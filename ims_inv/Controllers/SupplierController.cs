using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ims_inv.Models;
using ims_inv.Services;

namespace ims_inv.Controllers
{
    [Authorize]
    public class SupplierController : Controller
    {
        private readonly ISupplierService _supplierService;

        public SupplierController(ISupplierService supplierService)
        {
            _supplierService = supplierService;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["ActivePage"] = "Supplier";
            var suppliers = await _supplierService.GetAllSuppliersAsync();
            return View(suppliers);
        }

        [HttpGet]
        public async Task<IActionResult> CreateOrEdit(int id = 0)
        {
            ViewData["ActivePage"] = "Supplier";
            if (id == 0)
            {
                return View(new SupplierViewModel());
            }

            var viewModel = await _supplierService.GetSupplierViewModelAsync(id);
            if (viewModel == null)
            {
                return NotFound();
            }

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOrEdit(SupplierViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var (success, errorMessage, _) = await _supplierService.CreateOrUpdateSupplierAsync(model);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, errorMessage ?? "Error saving supplier.");
                return View(model);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _supplierService.DeleteSupplierAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
