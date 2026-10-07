using ims_inv.Models;

namespace ims_inv.Repositories
{
    public interface ISupplierRepository : IAbstractRepository<Supplier>
    {
        Task<List<Supplier>> GetActiveSuppliersAsync(CancellationToken cancellationToken = default);
    }
}
