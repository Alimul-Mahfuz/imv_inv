using Microsoft.EntityFrameworkCore;
using ims_inv.Data;
using ims_inv.Models;

namespace ims_inv.Repositories
{
    public class StockMovementRepository : AbstractRepository<StockMovement>, IStockMovementRepository
    {
        public StockMovementRepository(WebAppDbContext context) : base(context)
        {
        }

        public async Task<List<StockMovement>> GetAllWithDetailsAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(sm => sm.Product)
                    .ThenInclude(p => p.Unit)
                .Include(sm => sm.Warehouse)
                .Include(sm => sm.User)
                .OrderByDescending(sm => sm.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<StockMovement>> GetRecentMovementsAsync(int count = 10, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(sm => sm.Product)
                    .ThenInclude(p => p.Unit)
                .Include(sm => sm.Warehouse)
                .Include(sm => sm.User)
                .OrderByDescending(sm => sm.CreatedAt)
                .Take(count)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<StockMovement>> GetByProductAsync(int productId, int? warehouseId = null, CancellationToken cancellationToken = default)
        {
            var query = _dbSet
                .Include(sm => sm.Product)
                    .ThenInclude(p => p.Unit)
                .Include(sm => sm.Warehouse)
                .Include(sm => sm.User)
                .Where(sm => sm.ProductId == productId);

            if (warehouseId.HasValue)
            {
                query = query.Where(sm => sm.WarehouseId == warehouseId.Value);
            }

            return await query
                .OrderBy(sm => sm.CreatedAt)
                .ToListAsync(cancellationToken);
        }
    }
}
