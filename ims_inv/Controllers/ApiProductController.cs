using Microsoft.AspNetCore.Mvc;
using ims_inv.Services;

namespace ims_inv.Controllers
{
    [ApiController]
    [Route("api/products")]
    public class ApiProductController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly IUnitConversionService _unitConversionService;

        public ApiProductController(IProductService productService, IUnitConversionService unitConversionService)
        {
            _productService = productService;
            _unitConversionService = unitConversionService;
        }

        /// <summary>
        /// Get product info including base unit
        /// </summary>
        [HttpGet("{productId}")]
        public async Task<IActionResult> GetProduct(int productId)
        {
            var product = await _productService.GetProductByIdAsync(productId);
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
        /// </summary>
        [HttpGet("{productId}/convert")]
        public async Task<IActionResult> ConvertQuantity(int productId, int fromUnitId, decimal quantity)
        {
            try
            {
                var product = await _productService.GetProductByIdAsync(productId);
                if (product == null)
                    return NotFound(new { message = "Product not found" });

                if (fromUnitId == product.BaseUnitId)
                {
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

                var convertedQuantity = await _unitConversionService.ConvertToBaseUnitAsync(productId, quantity, fromUnitId);
                var factor = await _unitConversionService.GetConversionFactorAsync(productId, fromUnitId, product.BaseUnitId);

                return Ok(new
                {
                    quantity = quantity,
                    fromUnitId = fromUnitId,
                    toUnitId = product.BaseUnitId,
                    convertedQuantity = convertedQuantity,
                    conversionFactor = factor ?? 1m,
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
