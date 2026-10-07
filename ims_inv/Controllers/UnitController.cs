using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ims_inv.Models;
using ims_inv.Services;

namespace ims_inv.Controllers
{
    [Authorize]
    public class UnitController : Controller
    {
        private readonly IUnitService _unitService;

        public UnitController(IUnitService unitService)
        {
            _unitService = unitService;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["ActivePage"] = "Unit";
            var units = await _unitService.GetAllUnitsAsync();
            return View(units);
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
            ViewData["ActivePage"] = "Unit";
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
    }
}
