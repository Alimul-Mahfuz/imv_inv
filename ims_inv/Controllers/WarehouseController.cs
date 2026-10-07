using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ims_inv.Models;
using ims_inv.Services;

namespace ims_inv.Controllers
{
    [Authorize]
    public class WarehouseController : Controller
    {
        private readonly IWarehouseService _warehouseService;

        public WarehouseController(IWarehouseService warehouseService)
        {
            _warehouseService = warehouseService;
        }

        public IActionResult Index(int page = 1)
        {
            ViewData["ActivePage"] = "Warehouse";
            var pageSize = 10;
            var warehouses = _warehouseService.GetPagedWarehouses(page, pageSize);
            return View(warehouses);
        }

        [HttpGet]
        public async Task<IActionResult> CreateOrEdit(int id = 0)
        {
            ViewData["ActivePage"] = "Warehouse";
            if (id == 0)
            {
                return View(new WarehouseViewModel());
            }

            var viewModel = await _warehouseService.GetWarehouseViewModelAsync(id);
            if (viewModel == null)
            {
                return NotFound();
            }

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOrEdit(WarehouseViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var (success, errorMessage, _) = await _warehouseService.CreateOrUpdateWarehouseAsync(model);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, errorMessage ?? "Error saving warehouse.");
                return View(model);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _warehouseService.DeleteWarehouseAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
