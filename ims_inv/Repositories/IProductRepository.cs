using ims_inv.Models;

namespace ims_inv.Repositories
{
    public interface IProductRepository : IAbstractRepository<Product>
    {
        Task<List<Product>> GetAllWithDetailsAsync(CancellationToken cancellationToken = default);
        Task<Product?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> IsSkuTakenAsync(string sku, int? excludeProductId = null, CancellationToken cancellationToken = default);
    }
}
