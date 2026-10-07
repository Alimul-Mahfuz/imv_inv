using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ims_inv.Models;
using ims_inv.Services;

namespace ims_inv.Controllers
{
    [Authorize]
    public class UnitConversionController : Controller
    {
        private readonly IUnitConversionService _unitConversionService;
        private readonly IProductService _productService;
        private readonly IUnitService _unitService;

        public UnitConversionController(
            IUnitConversionService unitConversionService,
            IProductService productService,
            IUnitService unitService)
        {
            _unitConversionService = unitConversionService;
            _productService = productService;
            _unitService = unitService;
        }

        // GET: UnitConversion/Product/{productId}
        public async Task<IActionResult> ProductConversions(int productId)
        {
            ViewData["ActivePage"] = "Products";

            var product = await _productService.GetProductByIdAsync(productId);
            if (product == null)
                return NotFound();

            var conversions = await _unitConversionService.GetConversionsForProductAsync(productId);
            var allUnits = await _unitService.GetAllUnitsAsync();

            var model = new ProductUnitConversionViewModel
            {
                ProductId = product.Id,
                ProductName = product.Name,
                ProductSKU = product.SKU,
                BaseUnitId = product.BaseUnitId,
                BaseUnitName = product.Unit?.Name ?? string.Empty,
                BaseUnitSymbol = product.Unit?.Symbol ?? string.Empty,
                Conversions = conversions.Select(c => new UnitConversionViewModel
                {
                    Id = c.Id,
                    FromUnitId = c.FromUnitId,
                    FromUnitName = c.FromUnit?.Name ?? string.Empty,
                    FromUnitSymbol = c.FromUnit?.Symbol ?? string.Empty,
                    ToUnitId = c.ToUnitId,
                    ToUnitName = c.ToUnit?.Name ?? string.Empty,
                    ToUnitSymbol = c.ToUnit?.Symbol ?? string.Empty,
                    ConversionFactor = c.ConversionFactor
                }).ToList(),
                AllUnits = allUnits
            };

            return View(model);
        }

        // GET: UnitConversion/Create/{productId}
        public async Task<IActionResult> Create(int productId)
        {
            ViewData["ActivePage"] = "Products";

            var product = await _productService.GetProductByIdAsync(productId);
            if (product == null)
                return NotFound();

            var model = new CreateUnitConversionViewModel
            {
                ProductId = productId,
                ProductName = product.Name,
                BaseUnitId = product.BaseUnitId,
                BaseUnitName = product.Unit?.Name,
                AvailableUnits = await _unitService.GetAllUnitsAsync()
            };

            return View(model);
        }

        // POST: UnitConversion/Create/{productId}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int productId, CreateUnitConversionViewModel model)
        {
            ViewData["ActivePage"] = "Products";

            if (!ModelState.IsValid)
            {
                model.AvailableUnits = await _unitService.GetAllUnitsAsync();
                return View(model);
            }

            var product = await _productService.GetProductByIdAsync(productId);
            if (product == null)
                return NotFound();

            try
            {
                int fromUnitId = product.BaseUnitId;
                int toUnitId = model.TargetUnitId;

                var exists = await _unitConversionService.ConversionExistsAsync(productId, fromUnitId, toUnitId);
                if (exists)
                {
                    ModelState.AddModelError(string.Empty, "A conversion from this unit to the target unit already exists for this product.");
                    model.AvailableUnits = await _unitService.GetAllUnitsAsync();
                    return View(model);
                }

                await _unitConversionService.CreateConversionAsync(
                    productId,
                    fromUnitId,
                    toUnitId,
                    model.ConversionFactor);

                TempData["SuccessMessage"] = "Unit conversion created successfully!";
                return RedirectToAction(nameof(ProductConversions), new { productId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error: {ex.Message}");
                model.AvailableUnits = await _unitService.GetAllUnitsAsync();
                return View(model);
            }
        }

        // GET: UnitConversion/Edit/{conversionId}
        public async Task<IActionResult> Edit(int conversionId, int productId)
        {
            ViewData["ActivePage"] = "Products";

            var conversions = await _unitConversionService.GetConversionsForProductAsync(productId);
            var conversion = conversions.FirstOrDefault(uc => uc.Id == conversionId);

            if (conversion == null)
                return NotFound();

            var product = await _productService.GetProductByIdAsync(productId);
            if (product == null)
                return NotFound();

            var model = new EditUnitConversionViewModel
            {
                ConversionId = conversion.Id,
                ProductId = productId,
                FromUnitName = conversion.FromUnit?.Name ?? string.Empty,
                FromUnitSymbol = conversion.FromUnit?.Symbol ?? string.Empty,
                ToUnitName = conversion.ToUnit?.Name ?? string.Empty,
                ToUnitSymbol = conversion.ToUnit?.Symbol ?? string.Empty,
                ConversionFactor = conversion.ConversionFactor
            };

            return View(model);
        }

        // POST: UnitConversion/Edit/{conversionId}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int conversionId, int productId, EditUnitConversionViewModel model)
        {
            ViewData["ActivePage"] = "Products";

            if (!ModelState.IsValid)
                return View(model);

            try
            {
                await _unitConversionService.UpdateConversionAsync(conversionId, model.ConversionFactor);
                TempData["SuccessMessage"] = "Unit conversion updated successfully!";
                return RedirectToAction(nameof(ProductConversions), new { productId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error: {ex.Message}");
                return View(model);
            }
        }

        // POST: UnitConversion/Delete/{conversionId}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int conversionId, int productId)
        {
            try
            {
                await _unitConversionService.DeleteConversionAsync(conversionId);
                TempData["SuccessMessage"] = "Unit conversion deleted successfully!";
                return RedirectToAction(nameof(ProductConversions), new { productId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error: {ex.Message}";
                return RedirectToAction(nameof(ProductConversions), new { productId });
            }
        }
    }
}
