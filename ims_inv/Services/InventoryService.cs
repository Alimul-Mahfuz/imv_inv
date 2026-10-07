using ims_inv.Models;
using ims_inv.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ims_inv.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly IInventoryRepository _inventoryRepository;

        public InventoryService(IInventoryRepository inventoryRepository)
        {
            _inventoryRepository = inventoryRepository;
        }

        public async Task<List<Inventory>> GetAllInventoryAsync(int? warehouseId = null)
        {
            return await _inventoryRepository.GetAllWithProductAndWarehouseAsync(warehouseId);
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
    }
}
