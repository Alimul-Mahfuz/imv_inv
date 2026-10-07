using ims_inv.Models;

namespace ims_inv.Repositories
{
    public interface IStockMovementRepository : IAbstractRepository<StockMovement>
    {
        Task<List<StockMovement>> GetAllWithDetailsAsync(CancellationToken cancellationToken = default);
        Task<List<StockMovement>> GetRecentMovementsAsync(int count = 10, CancellationToken cancellationToken = default);
        Task<List<StockMovement>> GetByProductAsync(int productId, int? warehouseId = null, CancellationToken cancellationToken = default);
    }
}
