using ims_inv.Models;

namespace ims_inv.Repositories
{
    public interface IUnitConversionRepository : IAbstractRepository<UnitConversion>
    {
        Task<List<UnitConversion>> GetAllWithUnitsAsync(CancellationToken cancellationToken = default);
        Task<List<UnitConversion>> GetByProductIdWithUnitsAsync(int productId, CancellationToken cancellationToken = default);
        Task<UnitConversion?> GetConversionAsync(int productId, int fromUnitId, int toUnitId, CancellationToken cancellationToken = default);
    }
}
