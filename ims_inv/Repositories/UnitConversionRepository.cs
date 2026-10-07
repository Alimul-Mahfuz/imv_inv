using Microsoft.EntityFrameworkCore;
using ims_inv.Data;
using ims_inv.Models;

namespace ims_inv.Repositories
{
    public class UnitConversionRepository : AbstractRepository<UnitConversion>, IUnitConversionRepository
    {
        public UnitConversionRepository(WebAppDbContext context) : base(context)
        {
        }

        public async Task<List<UnitConversion>> GetAllWithUnitsAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(uc => uc.Product)
                .Include(uc => uc.FromUnit)
                .Include(uc => uc.ToUnit)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<UnitConversion>> GetByProductIdWithUnitsAsync(int productId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(uc => uc.ProductId == productId)
                .Include(uc => uc.FromUnit)
                .Include(uc => uc.ToUnit)
                .ToListAsync(cancellationToken);
        }

        public async Task<UnitConversion?> GetConversionAsync(int productId, int fromUnitId, int toUnitId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .FirstOrDefaultAsync(uc => uc.ProductId == productId &&
                                           uc.FromUnitId == fromUnitId &&
                                           uc.ToUnitId == toUnitId, cancellationToken);
        }
    }
}
