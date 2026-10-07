using ims_inv.Data;
using ims_inv.Models;
using ims_inv.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ims_inv.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly WebAppDbContext _context;
        private readonly IInventoryRepository _inventoryRepository;
        private readonly IProductRepository _productRepository;
        private readonly IWarehouseRepository _warehouseRepository;
        private readonly IStockMovementRepository _stockMovementRepository;
        private readonly IUnitConversionService _unitConversionService;

        public InventoryService(
            WebAppDbContext context,
            IInventoryRepository inventoryRepository,
            IProductRepository productRepository,
            IWarehouseRepository warehouseRepository,
            IStockMovementRepository stockMovementRepository,
            IUnitConversionService unitConversionService)
        {
            _context = context;
            _inventoryRepository = inventoryRepository;
            _productRepository = productRepository;
            _warehouseRepository = warehouseRepository;
            _stockMovementRepository = stockMovementRepository;
            _unitConversionService = unitConversionService;
        }

        public async Task<List<Inventory>> GetAllInventoryAsync(int? warehouseId = null)
        {
            return await _inventoryRepository.GetAllWithProductAndWarehouseAsync(warehouseId);
        }

        public async Task<Inventory?> GetInventoryByIdAsync(int id)
        {
            return await _inventoryRepository.GetByIdWithDetailsAsync(id);
        }

        public async Task<Inventory?> GetInventoryByProductAndWarehouseAsync(int productId, int warehouseId)
        {
            return await _inventoryRepository.GetByProductAndWarehouseAsync(productId, warehouseId);
        }

        public async Task<List<Inventory>> GetInventoryByProductIdAsync(int productId)
        {
            return await _inventoryRepository.GetByProductIdAsync(productId);
        }

        public async Task<decimal> GetTotalStockForProductAsync(int productId)
        {
            return await _inventoryRepository.Query()
                .Where(i => i.ProductId == productId)
                .SumAsync(i => (decimal?)i.Quantity) ?? 0m;
        }

        public async Task<List<Inventory>> GetLowStockAlertsAsync()
        {
            return await _inventoryRepository.GetLowStockAlertsAsync();
        }

        public async Task<bool> UpdateInventoryLevelsAsync(int id, decimal? reorderLevel, decimal? reorderQuantity)
        {
            var inventory = await _inventoryRepository.GetByIdAsync(id);
            if (inventory == null)
            {
                return false;
            }

            inventory.ReorderLevel = reorderLevel;
            inventory.ReorderQuantity = reorderQuantity;
            inventory.UpdatedAt = DateTime.UtcNow;

            _inventoryRepository.Update(inventory);
            await _inventoryRepository.SaveChangesAsync();
            return true;
        }

        public async Task UpdateReorderSettingsAsync(int productId, int warehouseId, decimal? reorderLevel, decimal? reorderQuantity)
        {
            var inventory = await _inventoryRepository.GetByProductAndWarehouseAsync(productId, warehouseId);
            if (inventory != null)
            {
                inventory.ReorderLevel = reorderLevel;
                inventory.ReorderQuantity = reorderQuantity;
                inventory.UpdatedAt = DateTime.UtcNow;

                _inventoryRepository.Update(inventory);
                await _inventoryRepository.SaveChangesAsync();
            }
        }

        public async Task<(bool Success, string? ErrorMessage)> TransferStockAsync(StockTransferViewModel model, int? currentUserId)
        {
            if (model.FromWarehouseId == model.ToWarehouseId)
            {
                return (false, "Source and destination warehouses cannot be the same.");
            }

            if (model.Quantity <= 0)
            {
                return (false, "Transfer quantity must be greater than zero.");
            }

            var product = await _productRepository.GetByIdAsync(model.ProductId);
            if (product == null)
            {
                return (false, "Product not found.");
            }

            var fromWarehouse = await _warehouseRepository.GetByIdAsync(model.FromWarehouseId);
            if (fromWarehouse == null)
            {
                return (false, "Source warehouse not found.");
            }

            var toWarehouse = await _warehouseRepository.GetByIdAsync(model.ToWarehouseId);
            if (toWarehouse == null)
            {
                return (false, "Destination warehouse not found.");
            }

            decimal quantityInBaseUnit = model.Quantity;
            if (model.EntryUnitId.HasValue && model.EntryUnitId.Value != product.BaseUnitId)
            {
                try
                {
                    quantityInBaseUnit = await _unitConversionService.ConvertAsync(
                        model.ProductId,
                        model.Quantity,
                        model.EntryUnitId.Value,
                        product.BaseUnitId
                    );
                }
                catch (Exception ex)
                {
                    return (false, $"Unit conversion failed: {ex.Message}");
                }
            }

            if (quantityInBaseUnit <= 0)
            {
                return (false, "Calculated transfer quantity must be greater than zero.");
            }

            const int maxRetries = 3;
            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var fromInventory = await _inventoryRepository.GetByProductAndWarehouseAsync(model.ProductId, model.FromWarehouseId);
                    var availableStock = fromInventory?.Quantity ?? 0m;

                    if (availableStock < quantityInBaseUnit)
                    {
                        await transaction.RollbackAsync();
                        return (false, $"Insufficient stock in source warehouse '{fromWarehouse.Name}'. Available: {availableStock:G29}, Requested: {quantityInBaseUnit:G29}.");
                    }

                    var toInventory = await _inventoryRepository.GetByProductAndWarehouseAsync(model.ProductId, model.ToWarehouseId);
                    if (toInventory == null)
                    {
                        toInventory = new Inventory
                        {
                            ProductId = model.ProductId,
                            WarehouseId = model.ToWarehouseId,
                            Quantity = 0m,
                            CreatedAt = DateTime.UtcNow,
                            LastCountedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };
                        await _inventoryRepository.AddAsync(toInventory);
                    }

                    // Deduct from source
                    fromInventory!.Quantity -= quantityInBaseUnit;
                    fromInventory.UpdatedAt = DateTime.UtcNow;
                    _inventoryRepository.Update(fromInventory);

                    // Add to destination
                    toInventory.Quantity += quantityInBaseUnit;
                    toInventory.UpdatedAt = DateTime.UtcNow;
                    _inventoryRepository.Update(toInventory);

                    await _inventoryRepository.SaveChangesAsync();

                    // Record OUT movement for source warehouse
                    var outMovement = new StockMovement
                    {
                        ProductId = model.ProductId,
                        WarehouseId = model.FromWarehouseId,
                        MovementType = "OUT",
                        Quantity = quantityInBaseUnit,
                        ReferenceNumber = model.ReferenceNumber,
                        Reason = $"Transfer OUT to {toWarehouse.Name}",
                        Notes = model.Notes,
                        UserId = currentUserId,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _stockMovementRepository.AddAsync(outMovement);

                    // Record IN movement for destination warehouse
                    var inMovement = new StockMovement
                    {
                        ProductId = model.ProductId,
                        WarehouseId = model.ToWarehouseId,
                        MovementType = "IN",
                        Quantity = quantityInBaseUnit,
                        ReferenceNumber = model.ReferenceNumber,
                        Reason = $"Transfer IN from {fromWarehouse.Name}",
                        Notes = model.Notes,
                        UserId = currentUserId,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _stockMovementRepository.AddAsync(inMovement);

                    await _stockMovementRepository.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return (true, null);
                }
                catch (DbUpdateConcurrencyException) when (attempt < maxRetries)
                {
                    await transaction.RollbackAsync();
                    await Task.Delay(50 * attempt);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return (false, $"Error processing stock transfer: {ex.Message}");
                }
            }

            return (false, "Could not complete stock transfer due to concurrent transactions. Please try again.");
        }

        public async Task<(bool Success, string? ErrorMessage)> AdjustStockAsync(StockAdjustmentViewModel model, int? currentUserId)
        {
            if (model.PhysicalCount < 0)
            {
                return (false, "Physical count cannot be negative.");
            }

            var product = await _productRepository.GetByIdAsync(model.ProductId);
            if (product == null)
            {
                return (false, "Product not found.");
            }

            var warehouse = await _warehouseRepository.GetByIdAsync(model.WarehouseId);
            if (warehouse == null)
            {
                return (false, "Warehouse not found.");
            }

            const int maxRetries = 3;
            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var inventory = await _inventoryRepository.GetByProductAndWarehouseAsync(model.ProductId, model.WarehouseId);
                    if (inventory == null)
                    {
                        inventory = new Inventory
                        {
                            ProductId = model.ProductId,
                            WarehouseId = model.WarehouseId,
                            Quantity = 0m,
                            CreatedAt = DateTime.UtcNow,
                            LastCountedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };
                        await _inventoryRepository.AddAsync(inventory);
                    }

                    decimal previousQuantity = inventory.Quantity;
                    decimal discrepancy = model.PhysicalCount - previousQuantity;

                    inventory.Quantity = model.PhysicalCount;
                    inventory.LastCountedAt = DateTime.UtcNow;
                    inventory.UpdatedAt = DateTime.UtcNow;
                    _inventoryRepository.Update(inventory);
                    await _inventoryRepository.SaveChangesAsync();

                    if (discrepancy != 0)
                    {
                        var movementType = discrepancy > 0 ? "IN" : "OUT";
                        var absDiscrepancy = Math.Abs(discrepancy);
                        var movement = new StockMovement
                        {
                            ProductId = model.ProductId,
                            WarehouseId = model.WarehouseId,
                            MovementType = movementType,
                            Quantity = absDiscrepancy,
                            ReferenceNumber = "ADJUSTMENT",
                            Reason = $"Stock Adjustment ({(discrepancy > 0 ? "+" : "-")}{absDiscrepancy:G29}): {model.Reason}",
                            Notes = model.Notes,
                            UserId = currentUserId,
                            CreatedAt = DateTime.UtcNow
                        };
                        await _stockMovementRepository.AddAsync(movement);
                        await _stockMovementRepository.SaveChangesAsync();
                    }

                    await transaction.CommitAsync();
                    return (true, null);
                }
                catch (DbUpdateConcurrencyException) when (attempt < maxRetries)
                {
                    await transaction.RollbackAsync();
                    await Task.Delay(50 * attempt);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return (false, $"Error processing stock adjustment: {ex.Message}");
                }
            }

            return (false, "Could not complete stock adjustment due to concurrency. Please try again.");
        }

        public async Task<StockCardViewModel?> GetStockCardAsync(int productId, int? warehouseId = null)
        {
            var product = await _productRepository.GetByIdWithDetailsAsync(productId);
            if (product == null)
            {
                return null;
            }

            var warehouses = await _warehouseRepository.GetActiveWarehousesAsync();
            var movements = await _stockMovementRepository.GetByProductAsync(productId, warehouseId);

            decimal runningBalance = 0m;
            var movementItems = new List<StockCardMovementItem>();

            foreach (var sm in movements)
            {
                decimal qtyIn = sm.MovementType == "IN" ? sm.Quantity : 0m;
                decimal qtyOut = sm.MovementType == "OUT" ? sm.Quantity : 0m;

                if (sm.MovementType == "IN")
                {
                    runningBalance += sm.Quantity;
                }
                else if (sm.MovementType == "OUT")
                {
                    runningBalance -= sm.Quantity;
                }

                movementItems.Add(new StockCardMovementItem
                {
                    Date = sm.CreatedAt,
                    MovementType = sm.MovementType,
                    WarehouseName = sm.Warehouse?.Name ?? "N/A",
                    QuantityIn = qtyIn,
                    QuantityOut = qtyOut,
                    RunningBalance = runningBalance,
                    ReferenceNumber = sm.ReferenceNumber,
                    Reason = sm.Reason,
                    Notes = sm.Notes,
                    RecordedBy = sm.User?.Name
                });
            }

            // Reverse for most recent on top display, but with calculated running balance
            movementItems.Reverse();

            decimal totalQty = warehouseId.HasValue
                ? (await _inventoryRepository.GetByProductAndWarehouseAsync(productId, warehouseId.Value))?.Quantity ?? 0m
                : await GetTotalStockForProductAsync(productId);

            return new StockCardViewModel
            {
                Product = product,
                SelectedWarehouseId = warehouseId,
                Warehouses = warehouses,
                TotalQuantity = totalQty,
                Movements = movementItems
            };
        }
    }
}
