using Microsoft.EntityFrameworkCore;
using ims_inv.Data;
using ims_inv.Models;

namespace ims_inv.Repositories
{
    public class CategoryRepository : AbstractRepository<Category>, ICategoryRepository
    {
        public CategoryRepository(WebAppDbContext context) : base(context)
        {
        }

        public async Task<List<Category>> GetAllWithParentAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(c => c.Parent)
                .ToListAsync(cancellationToken);
        }

        public async Task<Category?> GetByIdWithChildrenAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(c => c.Children)
                .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        }
    }
}
