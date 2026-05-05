using ims_inv.Data;
using ims_inv.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ims_inv.Controllers
{
    [ApiController]
    [Route("api/products")]
    public class ApiProductController : ControllerBase
    {
        private readonly WebAppDbContext _db;
        private readonly UnitConversionService _unitConversionService;

        public ApiProductController(WebAppDbContext db, UnitConversionService unitConversionService)
        {
            _db = db;
            _unitConversionService = unitConversionService;
        }

        /// <summary>
        /// Get product info including base unit
        /// </summary>
        [HttpGet("{productId}")]
        public async Task<IActionResult> GetProduct(int productId)
        {
            var product = await _db.Products
                .Include(p => p.Unit)
                .FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null)
                return NotFound(new { message = "Product not found" });

            return Ok(new
            {
                id = product.Id,
                name = product.Name,
                baseUnitId = product.BaseUnitId,
                baseUnitName = product.Unit?.Name ?? "Unknown"
            });
        }

        /// <summary>
        /// Convert quantity from entry unit to base unit for a product
        /// The UnitConversions table stores: FromUnitId (base) -> ToUnitId (other units) with factor
        /// So we invert the factor when converting FROM the other unit TO base unit
        /// Example: 1 Kg = 1000 Grams (factor 1000), so 1 Gram = 1/1000 Kg = 0.001 Kg
        /// </summary>
        [HttpGet("{productId}/convert")]
        public async Task<IActionResult> ConvertQuantity(int productId, int fromUnitId, decimal quantity)
        {
            try
            {
                var product = await _db.Products
                    .Include(p => p.Unit)
                    .FirstOrDefaultAsync(p => p.Id == productId);

                if (product == null)
                    return NotFound(new { message = "Product not found" });

                if (fromUnitId == product.BaseUnitId)
                {
                    // No conversion needed
                    return Ok(new
                    {
                        quantity = quantity,
                        fromUnitId = fromUnitId,
                        toUnitId = product.BaseUnitId,
                        convertedQuantity = quantity,
                        conversionFactor = 1m,
                        baseUnitName = product.Unit?.Name ?? "Unknown"
                    });
                }

                // Get conversion factor from UnitConversions table
                var conversion = await _db.UnitConversions
                    .FirstOrDefaultAsync(u => u.ProductId == productId &&
                                             u.FromUnitId == product.BaseUnitId &&
                                             u.ToUnitId == fromUnitId);

                if (conversion == null)
                {
                    return BadRequest(new
                    {
                        message = $"No conversion defined between unit {fromUnitId} and base unit {product.BaseUnitId} for this product"
                    });
                }

                // Invert the factor: if 1 Kg = 1000 Grams, then 1 Gram = 1/1000 Kg = 0.001 Kg
                decimal invertedFactor = conversion.ConversionFactor == 0 ? 0 : 1m / conversion.ConversionFactor;
                var convertedQuantity = quantity * invertedFactor;

                return Ok(new
                {
                    quantity = quantity,
                    fromUnitId = fromUnitId,
                    toUnitId = product.BaseUnitId,
                    convertedQuantity = convertedQuantity,
                    conversionFactor = invertedFactor,
                    baseUnitName = product.Unit?.Name ?? "Unknown"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
