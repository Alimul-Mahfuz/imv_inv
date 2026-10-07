using Microsoft.EntityFrameworkCore;
using ims_inv.Data;
using ims_inv.Models;

namespace ims_inv.Repositories
{
    public class WarehouseRepository : AbstractRepository<Warehouse>, IWarehouseRepository
    {
        public WarehouseRepository(WebAppDbContext context) : base(context)
        {
        }

        public async Task<List<Warehouse>> GetActiveWarehousesAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet.Where(w => w.IsActive).ToListAsync(cancellationToken);
        }
    }
}
