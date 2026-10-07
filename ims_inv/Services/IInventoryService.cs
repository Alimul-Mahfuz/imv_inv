using ims_inv.Models;

namespace ims_inv.Services
{
    public interface IInventoryService
    {
        Task<List<Inventory>> GetAllInventoryAsync(int? warehouseId = null);
        Task<Inventory?> GetInventoryByIdAsync(int id);
        Task<Inventory?> GetInventoryByProductAndWarehouseAsync(int productId, int warehouseId);
        Task<List<Inventory>> GetInventoryByProductIdAsync(int productId);
        Task<decimal> GetTotalStockForProductAsync(int productId);
        Task<List<Inventory>> GetLowStockAlertsAsync();
        Task<bool> UpdateInventoryLevelsAsync(int id, decimal? reorderLevel, decimal? reorderQuantity);
        Task UpdateReorderSettingsAsync(int productId, int warehouseId, decimal? reorderLevel, decimal? reorderQuantity);
        Task<(bool Success, string? ErrorMessage)> TransferStockAsync(StockTransferViewModel model, int? currentUserId);
        Task<(bool Success, string? ErrorMessage)> AdjustStockAsync(StockAdjustmentViewModel model, int? currentUserId);
        Task<StockCardViewModel?> GetStockCardAsync(int productId, int? warehouseId = null);
    }
}
