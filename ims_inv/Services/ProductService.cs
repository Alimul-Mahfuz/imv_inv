using ims_inv.Data;
using ims_inv.Models;
using ims_inv.Repositories;

namespace ims_inv.Services
{
    public class ProductService : IProductService
    {
        private readonly WebAppDbContext _context;
        private readonly IProductRepository _productRepository;
        private readonly IInventoryRepository _inventoryRepository;
        private readonly IWarehouseRepository _warehouseRepository;

        public ProductService(
            WebAppDbContext context,
            IProductRepository productRepository,
            IInventoryRepository inventoryRepository,
            IWarehouseRepository warehouseRepository)
        {
            _context = context;
            _productRepository = productRepository;
            _inventoryRepository = inventoryRepository;
            _warehouseRepository = warehouseRepository;
        }

        public async Task<List<Product>> GetAllProductsAsync()
        {
            return await _productRepository.GetAllWithDetailsAsync();
        }

        public async Task<Product?> GetProductByIdAsync(int id)
        {
            return await _productRepository.GetByIdWithDetailsAsync(id);
        }

        public async Task<ProductViewModel?> GetProductViewModelAsync(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                return null;
            }

            return new ProductViewModel
            {
                Id = product.Id,
                Name = product.Name,
                SKU = product.SKU,
                CategoryId = product.CategoryId,
                SupplierId = product.SupplierId,
                BaseUnitId = product.BaseUnitId
            };
        }

        public async Task<(bool Success, string? ErrorMessage, Product? Product)> CreateOrUpdateProductAsync(ProductViewModel model)
        {
            if (model.Id == 0)
            {
                if (await _productRepository.IsSkuTakenAsync(model.SKU))
                {
                    return (false, "SKU already exists.", null);
                }

                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var product = new Product
                    {
                        Name = model.Name,
                        SKU = model.SKU,
                        CategoryId = model.CategoryId,
                        SupplierId = model.SupplierId,
                        BaseUnitId = model.BaseUnitId,
                        CreatedAt = DateTime.UtcNow
                    };

                    await _productRepository.AddAsync(product);
                    await _productRepository.SaveChangesAsync();

                    // Auto-initialize inventory records for all active warehouses
                    var activeWarehouses = await _warehouseRepository.GetActiveWarehousesAsync();
                    foreach (var warehouse in activeWarehouses)
                    {
                        var initialInventory = new Inventory
                        {
                            ProductId = product.Id,
                            WarehouseId = warehouse.Id,
                            Quantity = 0m,
                            CreatedAt = DateTime.UtcNow,
                            LastCountedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };
                        await _inventoryRepository.AddAsync(initialInventory);
                    }

                    if (activeWarehouses.Any())
                    {
                        await _inventoryRepository.SaveChangesAsync();
                    }

                    await transaction.CommitAsync();
                    return (true, null, product);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return (false, $"Error creating product: {ex.Message}", null);
                }
            }
            else
            {
                var product = await _productRepository.GetByIdAsync(model.Id);
                if (product == null)
                {
                    return (false, "Product not found.", null);
                }

                if (await _productRepository.IsSkuTakenAsync(model.SKU, model.Id))
                {
                    return (false, "SKU already exists for another product.", null);
                }

                product.Name = model.Name;
                product.SKU = model.SKU;
                product.CategoryId = model.CategoryId;
                product.SupplierId = model.SupplierId;
                product.BaseUnitId = model.BaseUnitId;

                _productRepository.Update(product);
                await _productRepository.SaveChangesAsync();

                return (true, null, product);
            }
        }

        public async Task<bool> IsSkuUniqueAsync(string sku, int? excludeId = null)
        {
            return !await _productRepository.IsSkuTakenAsync(sku, excludeId);
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                return false;
            }

            await _productRepository.DeleteAsync(id);
            await _productRepository.SaveChangesAsync();
            return true;
        }
    }
}
