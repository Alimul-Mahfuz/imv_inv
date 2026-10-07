using ims_inv.Models;

namespace ims_inv.Services
{
    public interface IInventoryService
    {
        Task<List<Inventory>> GetAllInventoryAsync(int? warehouseId = null);
        Task<Inventory?> GetInventoryByProductAndWarehouseAsync(int productId, int warehouseId);
        Task<List<Inventory>> GetInventoryByProductIdAsync(int productId);
        Task<decimal> GetTotalStockForProductAsync(int productId);
        Task<List<Inventory>> GetLowStockAlertsAsync();
        Task UpdateReorderSettingsAsync(int productId, int warehouseId, decimal? reorderLevel, decimal? reorderQuantity);
    }
}
