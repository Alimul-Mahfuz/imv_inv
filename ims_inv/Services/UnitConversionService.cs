using ims_inv.Data;
using ims_inv.Models;
using Microsoft.EntityFrameworkCore;

namespace ims_inv.Services
{
    /// <summary>
    /// Service to handle unit conversions across the system
    /// </summary>
    public class UnitConversionService
    {
        private readonly WebAppDbContext _db;

        public UnitConversionService(WebAppDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Convert a quantity from one unit to another for a specific product
        /// Database stores conversions as: FromUnitId (base) -> ToUnitId (other units)
        /// So we invert the factor when converting FROM other unit TO base unit
        /// </summary>
        /// <param name="productId">Product ID to scope conversion</param>
        /// <param name="quantity">The quantity to convert</param>
        /// <param name="fromUnitId">Source unit ID</param>
        /// <param name="toUnitId">Target unit ID</param>
        /// <returns>Converted quantity</returns>
        /// <exception cref="Exception">Throws if conversion path doesn't exist</exception>
        public async Task<decimal> ConvertAsync(int productId, decimal quantity, int fromUnitId, int toUnitId)
        {
            // Same unit, no conversion needed
            if (fromUnitId == toUnitId)
                return quantity;

            var product = await _db.Products.FindAsync(productId);
            if (product == null)
                throw new InvalidOperationException($"Product with ID {productId} not found");

            // Case 1: Converting FROM base unit TO another unit
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

            // Case 2: Converting FROM another unit TO base unit (need to invert)
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

            // Case 3: Converting between two non-base units (convert via base unit)
            throw new InvalidOperationException(
                $"Conversion between non-base units is not supported. Use base unit as intermediate.");
        }

        /// <summary>
        /// Get all available conversions
        /// </summary>
        public async Task<List<UnitConversion>> GetAllConversionsAsync()
        {
            return await _db.UnitConversions
                .Include(uc => uc.FromUnit)
                .Include(uc => uc.ToUnit)
                .ToListAsync();
        }

        /// <summary>
        /// Get conversions for a specific product
        /// </summary>
        public async Task<List<UnitConversion>> GetConversionsForProductAsync(int productId)
        {
            return await _db.UnitConversions
                .Where(uc => uc.ProductId == productId)
                .Include(uc => uc.FromUnit)
                .Include(uc => uc.ToUnit)
                .ToListAsync();
        }

        /// <summary>
        /// Get conversions from a specific unit for a product
        /// </summary>
        public async Task<List<UnitConversion>> GetConversionsFromUnitAsync(int productId, int fromUnitId)
        {
            return await _db.UnitConversions
                .Where(uc => uc.ProductId == productId && uc.FromUnitId == fromUnitId)
                .Include(uc => uc.FromUnit)
                .Include(uc => uc.ToUnit)
                .ToListAsync();
        }

        /// <summary>
        /// Create a new unit conversion for a product
        /// </summary>
        public async Task<UnitConversion> CreateConversionAsync(int productId, int fromUnitId, int toUnitId, decimal factor)
        {
            // Validate product exists
            var product = await _db.Products.FindAsync(productId);
            if (product == null)
                throw new InvalidOperationException($"Product with ID {productId} not found");

            // Validate units exist
            var fromUnit = await _db.Units.FindAsync(fromUnitId);
            var toUnit = await _db.Units.FindAsync(toUnitId);

            if (fromUnit == null)
                throw new InvalidOperationException($"From Unit with ID {fromUnitId} not found");

            if (toUnit == null)
                throw new InvalidOperationException($"To Unit with ID {toUnitId} not found");

            // Check if conversion already exists for this product
            var existing = await _db.UnitConversions
                .FirstOrDefaultAsync(uc => uc.ProductId == productId && uc.FromUnitId == fromUnitId && uc.ToUnitId == toUnitId);

            if (existing != null)
                throw new InvalidOperationException(
                    $"Conversion from {fromUnit.Name} to {toUnit.Name} already exists for product {product.Name}");

            var conversion = new UnitConversion
            {
                ProductId = productId,
                FromUnitId = fromUnitId,
                ToUnitId = toUnitId,
                ConversionFactor = factor,
                CreatedAt = DateTime.Now
            };

            _db.UnitConversions.Add(conversion);
            await _db.SaveChangesAsync();

            return conversion;
        }

        /// <summary>
        /// Update an existing conversion
        /// </summary>
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

        /// <summary>
        /// Delete a conversion
        /// </summary>
        public async Task DeleteConversionAsync(int id)
        {
            var conversion = await _db.UnitConversions.FindAsync(id);

            if (conversion == null)
                throw new InvalidOperationException($"Conversion with ID {id} not found");

            _db.UnitConversions.Remove(conversion);
            await _db.SaveChangesAsync();
        }

        /// <summary>
        /// Check if a conversion path exists between two units for a product
        /// </summary>
        public async Task<bool> ConversionExistsAsync(int productId, int fromUnitId, int toUnitId)
        {
            if (fromUnitId == toUnitId)
                return true;

            var product = await _db.Products.FindAsync(productId);
            if (product == null)
                return false;

            // Case 1: Converting FROM base unit TO another unit
            if (fromUnitId == product.BaseUnitId)
            {
                return await _db.UnitConversions
                    .AnyAsync(uc => uc.ProductId == productId && 
                                   uc.FromUnitId == fromUnitId && 
                                   uc.ToUnitId == toUnitId);
            }

            // Case 2: Converting FROM another unit TO base unit
            if (toUnitId == product.BaseUnitId)
            {
                return await _db.UnitConversions
                    .AnyAsync(uc => uc.ProductId == productId && 
                                   uc.FromUnitId == product.BaseUnitId && 
                                   uc.ToUnitId == fromUnitId);
            }

            // Case 3: Converting between two non-base units (not supported)
            return false;
        }

        /// <summary>
        /// Get conversion factor between two units for a product
        /// Returns inverted factor when converting FROM non-base TO base unit
        /// </summary>
        public async Task<decimal?> GetConversionFactorAsync(int productId, int fromUnitId, int toUnitId)
        {
            if (fromUnitId == toUnitId)
                return 1m;

            var product = await _db.Products.FindAsync(productId);
            if (product == null)
                return null;

            // Case 1: Converting FROM base unit TO another unit
            if (fromUnitId == product.BaseUnitId)
            {
                var conversion = await _db.UnitConversions
                    .FirstOrDefaultAsync(uc => uc.ProductId == productId && 
                                              uc.FromUnitId == fromUnitId && 
                                              uc.ToUnitId == toUnitId);
                return conversion?.ConversionFactor;
            }

            // Case 2: Converting FROM another unit TO base unit (invert)
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

            // Case 3: Converting between two non-base units (not supported)
            return null;
        }

        /// <summary>
        /// Convert product quantity to base unit
        /// </summary>
        public async Task<decimal> ConvertToBaseUnitAsync(int productId, decimal quantity, int fromUnitId)
        {
            var product = await _db.Products.FindAsync(productId);

            if (product == null)
                throw new InvalidOperationException($"Product with ID {productId} not found");

            return await ConvertAsync(productId, quantity, fromUnitId, product.BaseUnitId);
        }

        /// <summary>
        /// Convert product quantity from base unit
        /// </summary>
        public async Task<decimal> ConvertFromBaseUnitAsync(int productId, decimal quantity, int toUnitId)
        {
            var product = await _db.Products.FindAsync(productId);

            if (product == null)
                throw new InvalidOperationException($"Product with ID {productId} not found");

            return await ConvertAsync(productId, quantity, product.BaseUnitId, toUnitId);
        }
    }
}
