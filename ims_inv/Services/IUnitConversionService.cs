using ims_inv.Models;

namespace ims_inv.Services
{
    public interface IUnitConversionService
    {
        Task<decimal> ConvertAsync(int productId, decimal quantity, int fromUnitId, int toUnitId);
        Task<List<UnitConversion>> GetAllConversionsAsync();
        Task<List<UnitConversion>> GetConversionsForProductAsync(int productId);
        Task<List<UnitConversion>> GetConversionsFromUnitAsync(int productId, int fromUnitId);
        Task<UnitConversion> CreateConversionAsync(int productId, int fromUnitId, int toUnitId, decimal factor);
        Task<UnitConversion> UpdateConversionAsync(int id, decimal newFactor);
        Task DeleteConversionAsync(int id);
        Task<bool> ConversionExistsAsync(int productId, int fromUnitId, int toUnitId);
        Task<decimal?> GetConversionFactorAsync(int productId, int fromUnitId, int toUnitId);
        Task<decimal> ConvertToBaseUnitAsync(int productId, decimal quantity, int fromUnitId);
        Task<decimal> ConvertFromBaseUnitAsync(int productId, decimal quantity, int toUnitId);
    }
}
