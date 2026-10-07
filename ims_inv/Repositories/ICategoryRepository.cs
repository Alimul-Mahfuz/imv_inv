using ims_inv.Models;

namespace ims_inv.Repositories
{
    public interface ICategoryRepository : IAbstractRepository<Category>
    {
        Task<List<Category>> GetAllWithParentAsync(CancellationToken cancellationToken = default);
        Task<Category?> GetByIdWithChildrenAsync(int id, CancellationToken cancellationToken = default);
    }
}
