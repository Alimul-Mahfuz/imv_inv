using Microsoft.EntityFrameworkCore;
using ims_inv.Data;
using ims_inv.Models;

namespace ims_inv.Repositories
{
    public class SupplierRepository : AbstractRepository<Supplier>, ISupplierRepository
    {
        public SupplierRepository(WebAppDbContext context) : base(context)
        {
        }

        public async Task<List<Supplier>> GetActiveSuppliersAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet.Where(s => s.IsActive).ToListAsync(cancellationToken);
        }
    }
}
