using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ims_inv.Controllers;
using ims_inv.Models;
using ims_inv.Services;

namespace ims_inv.Controllers
{
    [Authorize]
    public class UnitController : Controller
    {
        private readonly IUnitService _unitService;
        private readonly IUnitConversionService _unitConversionService;
        private readonly IProductService _productService;

        public UnitController(
            IUnitService unitService,
            IUnitConversionService unitConversionService,
            IProductService productService)
        {
            _unitService = unitService;
            _unitConversionService = unitConversionService;
            _productService = productService;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["ActivePage"] = "Unit";
            var units = await _unitService.GetAllUnitsAsync();
            var conversions = await _unitConversionService.GetAllConversionsAsync();
            var products = await _productService.GetAllProductsAsync();

            var viewModel = new UnitManagementViewModel
            {
                Units = units,
                Conversions = conversions,
                Products = products,
                NewConversion = new CreateUnitConversionViewModel
                {
                    AvailableUnits = units
                }
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> CreateOrEdit(int id = 0)
        {
            ViewData["ActivePage"] = "Unit";
            if (id == 0)
            {
                return View(new UnitViewModel());
            }

            var viewModel = await _unitService.GetUnitViewModelAsync(id);
            if (viewModel == null)
            {
                return NotFound();
            }

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOrEdit(UnitViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var (success, errorMessage, _) = await _unitService.CreateOrUpdateUnitAsync(model);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, errorMessage ?? "Error saving unit.");
                return View(model);
            }

            TempData["SuccessMessage"] = model.Id == 0 ? "Unit created successfully!" : "Unit updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _unitService.DeleteUnitAsync(id);
            TempData["SuccessMessage"] = "Unit deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateConversion(CreateUnitConversionViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Please fill in all conversion fields correctly.";
                return RedirectToAction(nameof(Index));
            }

            var product = await _productService.GetProductByIdAsync(model.ProductId);
            if (product == null)
            {
                TempData["ErrorMessage"] = "Selected product was not found.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                int fromUnitId = product.BaseUnitId;
                int toUnitId = model.TargetUnitId;

                if (fromUnitId == toUnitId)
                {
                    TempData["ErrorMessage"] = "Target unit cannot be the same as the product's base unit.";
                    return RedirectToAction(nameof(Index));
                }

                var exists = await _unitConversionService.ConversionExistsAsync(model.ProductId, fromUnitId, toUnitId);
                if (exists)
                {
                    TempData["ErrorMessage"] = "A conversion for this product and unit combination already exists.";
                    return RedirectToAction(nameof(Index));
                }

                await _unitConversionService.CreateConversionAsync(
                    model.ProductId,
                    fromUnitId,
                    toUnitId,
                    model.ConversionFactor);

                TempData["SuccessMessage"] = "Unit conversion configured successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error creating unit conversion: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConversion(int conversionId)
        {
            try
            {
                await _unitConversionService.DeleteConversionAsync(conversionId);
                TempData["SuccessMessage"] = "Unit conversion deleted successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error deleting conversion: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
