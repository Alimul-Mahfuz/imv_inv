using ims_inv.Models;

namespace ims_inv.Repositories
{
    public interface IInventoryRepository : IAbstractRepository<Inventory>
    {
        Task<List<Inventory>> GetAllWithProductAndWarehouseAsync(int? warehouseId = null, CancellationToken cancellationToken = default);
        Task<Inventory?> GetByProductAndWarehouseAsync(int productId, int warehouseId, CancellationToken cancellationToken = default);
        Task<List<Inventory>> GetByProductIdAsync(int productId, CancellationToken cancellationToken = default);
        Task<List<Inventory>> GetLowStockAlertsAsync(CancellationToken cancellationToken = default);
    }
}
