using ims_inv.Models;

namespace ims_inv.Repositories
{
    public interface IWarehouseRepository : IAbstractRepository<Warehouse>
    {
        Task<List<Warehouse>> GetActiveWarehousesAsync(CancellationToken cancellationToken = default);
    }
}
