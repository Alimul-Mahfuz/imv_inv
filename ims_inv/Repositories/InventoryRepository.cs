using Microsoft.EntityFrameworkCore;
using ims_inv.Data;
using ims_inv.Models;

namespace ims_inv.Repositories
{
    public class InventoryRepository : AbstractRepository<Inventory>, IInventoryRepository
    {
        public InventoryRepository(WebAppDbContext context) : base(context)
        {
        }

        public async Task<List<Inventory>> GetAllWithProductAndWarehouseAsync(int? warehouseId = null, CancellationToken cancellationToken = default)
        {
            var query = _dbSet
                .Include(i => i.Product)
                    .ThenInclude(p => p!.Unit)
                .Include(i => i.Product)
                    .ThenInclude(p => p!.Category)
                .Include(i => i.Warehouse)
                .AsQueryable();

            if (warehouseId.HasValue)
            {
                query = query.Where(i => i.WarehouseId == warehouseId.Value);
            }

            return await query
                .OrderBy(i => i.Product!.Name)
                .ThenBy(i => i.Warehouse!.Name)
                .ToListAsync(cancellationToken);
        }

        public async Task<Inventory?> GetByProductAndWarehouseAsync(int productId, int warehouseId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(i => i.Product)
                    .ThenInclude(p => p!.Unit)
                .Include(i => i.Warehouse)
                .FirstOrDefaultAsync(i => i.ProductId == productId && i.WarehouseId == warehouseId, cancellationToken);
        }

        public async Task<List<Inventory>> GetByProductIdAsync(int productId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(i => i.Warehouse)
                .Where(i => i.ProductId == productId)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<Inventory>> GetLowStockAlertsAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(i => i.Product)
                    .ThenInclude(p => p!.Unit)
                .Include(i => i.Warehouse)
                .Where(i => i.ReorderLevel.HasValue && i.Quantity <= i.ReorderLevel.Value)
                .ToListAsync(cancellationToken);
        }
    }
}
