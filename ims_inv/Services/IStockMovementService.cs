using ims_inv.Models;

namespace ims_inv.Services
{
    public interface IStockMovementService
    {
        Task<List<StockMovement>> GetAllMovementsAsync();
        Task<List<StockMovement>> GetRecentMovementsAsync(int count = 10);
        Task<(bool Success, string? ErrorMessage, StockMovement? Movement)> RecordStockMovementAsync(CreateStockMovementViewModel model, int? currentUserId);
    }
}
