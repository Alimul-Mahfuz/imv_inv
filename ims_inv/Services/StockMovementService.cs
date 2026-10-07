using ims_inv.Data;
using ims_inv.Models;
using ims_inv.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ims_inv.Services
{
    public class StockMovementService : IStockMovementService
    {
        private readonly WebAppDbContext _context;
        private readonly IStockMovementRepository _stockMovementRepository;
        private readonly IInventoryRepository _inventoryRepository;
        private readonly IProductRepository _productRepository;
        private readonly IWarehouseRepository _warehouseRepository;
        private readonly IUnitConversionService _unitConversionService;

        public StockMovementService(
            WebAppDbContext context,
            IStockMovementRepository stockMovementRepository,
            IInventoryRepository inventoryRepository,
            IProductRepository productRepository,
            IWarehouseRepository warehouseRepository,
            IUnitConversionService unitConversionService)
        {
            _context = context;
            _stockMovementRepository = stockMovementRepository;
            _inventoryRepository = inventoryRepository;
            _productRepository = productRepository;
            _warehouseRepository = warehouseRepository;
            _unitConversionService = unitConversionService;
        }

        public async Task<List<StockMovement>> GetAllMovementsAsync()
        {
            return await _stockMovementRepository.GetAllWithDetailsAsync();
        }

        public async Task<List<StockMovement>> GetRecentMovementsAsync(int count = 10)
        {
            return await _stockMovementRepository.GetRecentMovementsAsync(count);
        }

        public async Task<(bool Success, string? ErrorMessage, StockMovement? Movement)> RecordStockMovementAsync(CreateStockMovementViewModel model, int? currentUserId)
        {
            if (model.Quantity <= 0)
            {
                return (false, "Quantity must be greater than zero.", null);
            }

            var movementType = model.MovementType?.Trim().ToUpperInvariant();
            if (movementType != "IN" && movementType != "OUT")
            {
                return (false, "Movement type must be either 'IN' or 'OUT'.", null);
            }

            var product = await _productRepository.GetByIdAsync(model.ProductId);
            if (product == null)
            {
                return (false, "Product not found.", null);
            }

            var warehouse = await _warehouseRepository.GetByIdAsync(model.WarehouseId);
            if (warehouse == null)
            {
                return (false, "Warehouse not found.", null);
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
                    return (false, $"Unit conversion failed: {ex.Message}", null);
                }
            }

            if (quantityInBaseUnit <= 0)
            {
                return (false, "Calculated quantity in base unit must be greater than zero.", null);
            }

            const int maxRetries = 3;
            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var inventory = await _inventoryRepository.GetByProductAndWarehouseAsync(model.ProductId, model.WarehouseId);

                    // Validate negative stock for OUT transactions at this specific warehouse
                    if (movementType == "OUT")
                    {
                        var availableStock = inventory?.Quantity ?? 0m;
                        if (availableStock < quantityInBaseUnit)
                        {
                            await transaction.RollbackAsync();
                            var unitSymbol = product.Unit?.Symbol ?? "units";
                            return (false, $"Insufficient stock in '{warehouse.Name}'. Available: {availableStock:G29} {unitSymbol}, Requested: {quantityInBaseUnit:G29} {unitSymbol}.", null);
                        }
                    }

                    if (inventory == null)
                    {
                        inventory = new Inventory
                        {
                            ProductId = model.ProductId,
                            WarehouseId = model.WarehouseId,
                            Quantity = quantityInBaseUnit, // Validated IN movement
                            LastCountedAt = DateTime.UtcNow,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };
                        await _inventoryRepository.AddAsync(inventory);
                    }
                    else
                    {
                        if (movementType == "IN")
                        {
                            inventory.Quantity += quantityInBaseUnit;
                        }
                        else if (movementType == "OUT")
                        {
                            inventory.Quantity -= quantityInBaseUnit;
                        }

                        inventory.LastCountedAt = DateTime.UtcNow;
                        inventory.UpdatedAt = DateTime.UtcNow;
                        _inventoryRepository.Update(inventory);
                    }

                    await _inventoryRepository.SaveChangesAsync();

                    var movement = new StockMovement
                    {
                        ProductId = model.ProductId,
                        WarehouseId = model.WarehouseId,
                        MovementType = movementType,
                        Quantity = quantityInBaseUnit,
                        ReferenceNumber = model.ReferenceNumber,
                        Reason = model.Reason,
                        Notes = model.Notes,
                        UserId = currentUserId,
                        CreatedAt = DateTime.UtcNow
                    };

                    await _stockMovementRepository.AddAsync(movement);
                    await _stockMovementRepository.SaveChangesAsync();

                    await transaction.CommitAsync();

                    return (true, null, movement);
                }
                catch (DbUpdateConcurrencyException) when (attempt < maxRetries)
                {
                    await transaction.RollbackAsync();
                    await Task.Delay(50 * attempt);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return (false, $"Error processing stock movement: {ex.Message}", null);
                }
            }

            return (false, "Could not complete stock movement due to high concurrency. Please try again.", null);
        }
    }
}
