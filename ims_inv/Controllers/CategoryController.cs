using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ims_inv.Models;
using ims_inv.Services;

namespace ims_inv.Controllers
{
    [Authorize]
    public class CategoryController : Controller
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["ActivePage"] = "Category";
            var categories = await _categoryService.GetAllCategoriesAsync();
            return View(categories);
        }

        [HttpGet]
        public async Task<IActionResult> CreateOrEdit(int id = 0)
        {
            ViewData["ActivePage"] = "Category";

            var eligibleParents = await _categoryService.GetEligibleParentCategoriesAsync(id);
            ViewBag.ParentId = new SelectList(eligibleParents, "Id", "Name");

            if (id == 0)
            {
                return View(new CategoryViewModel());
            }

            var viewModel = await _categoryService.GetCategoryViewModelAsync(id);
            if (viewModel == null)
            {
                return NotFound();
            }

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOrEdit(CategoryViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var eligibleParents = await _categoryService.GetEligibleParentCategoriesAsync(model.Id);
                ViewBag.ParentId = new SelectList(eligibleParents, "Id", "Name", model.ParentId);
                return View(model);
            }

            var (success, errorMessage, _) = await _categoryService.CreateOrUpdateCategoryAsync(model);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, errorMessage ?? "Error saving category.");
                var eligibleParents = await _categoryService.GetEligibleParentCategoriesAsync(model.Id);
                ViewBag.ParentId = new SelectList(eligibleParents, "Id", "Name", model.ParentId);
                return View(model);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var (success, errorMessage) = await _categoryService.DeleteCategoryAsync(id);
            if (!success)
            {
                TempData["Error"] = errorMessage ?? "Cannot delete category.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
