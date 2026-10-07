using Microsoft.EntityFrameworkCore;
using ims_inv.Data;
using ims_inv.Models;

namespace ims_inv.Repositories
{
    public class ProductRepository : AbstractRepository<Product>, IProductRepository
    {
        public ProductRepository(WebAppDbContext context) : base(context)
        {
        }

        public async Task<List<Product>> GetAllWithDetailsAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .Include(p => p.Unit)
                .ToListAsync(cancellationToken);
        }

        public async Task<Product?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .Include(p => p.Unit)
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        }

        public async Task<bool> IsSkuTakenAsync(string sku, int? excludeProductId = null, CancellationToken cancellationToken = default)
        {
            if (excludeProductId.HasValue)
            {
                return await _dbSet.AnyAsync(p => p.SKU == sku && p.Id != excludeProductId.Value, cancellationToken);
            }
            return await _dbSet.AnyAsync(p => p.SKU == sku, cancellationToken);
        }
    }
}
