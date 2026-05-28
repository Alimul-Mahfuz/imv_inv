using ims_inv.Data;
using ims_inv.Models;
using Microsoft.EntityFrameworkCore;

namespace ims_inv.Services
{

    public class UnitConversionService
    {
        private readonly WebAppDbContext _db;

        public UnitConversionService(WebAppDbContext db)
        {
            _db = db;
        }

        public async Task<decimal> ConvertAsync(int productId, decimal quantity, int fromUnitId, int toUnitId)
        {
            if (fromUnitId == toUnitId)
                return quantity;

            var product = await _db.Products.FindAsync(productId);
            if (product == null)
                throw new InvalidOperationException($"Product with ID {productId} not found");

            if (fromUnitId == product.BaseUnitId)
            {
                var conversion = await _db.UnitConversions
                    .FirstOrDefaultAsync(uc => uc.ProductId == productId &&
                                              uc.FromUnitId == fromUnitId &&
                                              uc.ToUnitId == toUnitId);

                if (conversion == null)
                    throw new InvalidOperationException(
                        $"No conversion defined for product {productId} from unit {fromUnitId} to unit {toUnitId}");

                return quantity * conversion.ConversionFactor;
            }

            if (toUnitId == product.BaseUnitId)
            {
                var conversion = await _db.UnitConversions
                    .FirstOrDefaultAsync(uc => uc.ProductId == productId &&
                                              uc.FromUnitId == product.BaseUnitId &&
                                              uc.ToUnitId == fromUnitId);

                if (conversion == null)
                    throw new InvalidOperationException(
                        $"No conversion defined for product {productId} from unit {fromUnitId} to unit {toUnitId}");

                decimal invertedFactor = 1m / conversion.ConversionFactor;
                return quantity * invertedFactor;
            }
            throw new InvalidOperationException(
                $"Conversion between non-base units is not supported. Use base unit as intermediate.");
        }

        public async Task<List<UnitConversion>> GetAllConversionsAsync()
        {
            return await _db.UnitConversions
                .Include(uc => uc.FromUnit)
                .Include(uc => uc.ToUnit)
                .ToListAsync();
        }


        public async Task<List<UnitConversion>> GetConversionsForProductAsync(int productId)
        {
            return await _db.UnitConversions
                .Where(uc => uc.ProductId == productId)
                .Include(uc => uc.FromUnit)
                .Include(uc => uc.ToUnit)
                .ToListAsync();
        }


        public async Task<List<UnitConversion>> GetConversionsFromUnitAsync(int productId, int fromUnitId)
        {
            return await _db.UnitConversions
                .Where(uc => uc.ProductId == productId && uc.FromUnitId == fromUnitId)
                .Include(uc => uc.FromUnit)
                .Include(uc => uc.ToUnit)
                .ToListAsync();
        }


        public async Task<UnitConversion> CreateConversionAsync(int productId, int fromUnitId, int toUnitId, decimal factor)
        {
            var product = await _db.Products.FindAsync(productId);
            if (product == null)
                throw new InvalidOperationException($"Product with ID {productId} not found");

            var fromUnit = await _db.Units.FindAsync(fromUnitId);
            var toUnit = await _db.Units.FindAsync(toUnitId);

            if (fromUnit == null)
                throw new InvalidOperationException($"From Unit with ID {fromUnitId} not found");

            if (toUnit == null)
                throw new InvalidOperationException($"To Unit with ID {toUnitId} not found");

            var conversion = new UnitConversion
            {
                ProductId = productId,
                FromUnitId = fromUnitId,
                ToUnitId = toUnitId,
                ConversionFactor = factor,
                CreatedAt = DateTime.UtcNow
            };

            _db.UnitConversions.Add(conversion);
            await _db.SaveChangesAsync();

            return conversion;
        }

        public async Task<UnitConversion> UpdateConversionAsync(int id, decimal newFactor)
        {
            var conversion = await _db.UnitConversions.FindAsync(id);

            if (conversion == null)
                throw new InvalidOperationException($"Conversion with ID {id} not found");

            conversion.ConversionFactor = newFactor;
            conversion.UpdatedAt = DateTime.Now;

            await _db.SaveChangesAsync();

            return conversion;
        }


        public async Task DeleteConversionAsync(int id)
        {
            var conversion = await _db.UnitConversions.FindAsync(id);

            if (conversion == null)
                throw new InvalidOperationException($"Conversion with ID {id} not found");

            _db.UnitConversions.Remove(conversion);
            await _db.SaveChangesAsync();
        }

        public async Task<bool> ConversionExistsAsync(int productId, int fromUnitId, int toUnitId)
        {
            if (fromUnitId == toUnitId)
                return true;

            var product = await _db.Products.FindAsync(productId);
            if (product == null)
                return false;

            if (fromUnitId == product.BaseUnitId)
            {
                return await _db.UnitConversions
                    .AnyAsync(uc => uc.ProductId == productId &&
                                   uc.FromUnitId == fromUnitId &&
                                   uc.ToUnitId == toUnitId);
            }

            if (toUnitId == product.BaseUnitId)
            {
                return await _db.UnitConversions
                    .AnyAsync(uc => uc.ProductId == productId &&
                                   uc.FromUnitId == product.BaseUnitId &&
                                   uc.ToUnitId == fromUnitId);
            }

            return false;
        }


        public async Task<decimal?> GetConversionFactorAsync(int productId, int fromUnitId, int toUnitId)
        {
            if (fromUnitId == toUnitId)
                return 1m;

            var product = await _db.Products.FindAsync(productId);
            if (product == null)
                return null;

            if (fromUnitId == product.BaseUnitId)
            {
                var conversion = await _db.UnitConversions
                    .FirstOrDefaultAsync(uc => uc.ProductId == productId &&
                                              uc.FromUnitId == fromUnitId &&
                                              uc.ToUnitId == toUnitId);
                return conversion?.ConversionFactor;
            }

            if (toUnitId == product.BaseUnitId)
            {
                var conversion = await _db.UnitConversions
                    .FirstOrDefaultAsync(uc => uc.ProductId == productId &&
                                              uc.FromUnitId == product.BaseUnitId &&
                                              uc.ToUnitId == fromUnitId);
                if (conversion == null)
                    return null;
                return 1m / conversion.ConversionFactor;
            }

            return null;
        }


        public async Task<decimal> ConvertToBaseUnitAsync(int productId, decimal quantity, int fromUnitId)
        {
            var product = await _db.Products.FindAsync(productId);

            if (product == null)
                throw new InvalidOperationException($"Product with ID {productId} not found");

            return await ConvertAsync(productId, quantity, fromUnitId, product.BaseUnitId);
        }


        public async Task<decimal> ConvertFromBaseUnitAsync(int productId, decimal quantity, int toUnitId)
        {
            var product = await _db.Products.FindAsync(productId);

            if (product == null)
                throw new InvalidOperationException($"Product with ID {productId} not found");

            return await ConvertAsync(productId, quantity, product.BaseUnitId, toUnitId);
        }
    }
}
