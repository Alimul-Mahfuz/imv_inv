using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ims_inv.Models;
using ims_inv.Services;

namespace ims_inv.Controllers
{
    [Authorize]
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly ISupplierService _supplierService;
        private readonly IUnitService _unitService;

        public ProductController(
            IProductService productService,
            ICategoryService categoryService,
            ISupplierService supplierService,
            IUnitService unitService)
        {
            _productService = productService;
            _categoryService = categoryService;
            _supplierService = supplierService;
            _unitService = unitService;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["ActivePage"] = "Product";
            var products = await _productService.GetAllProductsAsync();
            return View(products);
        }

        public async Task<IActionResult> CreateOrEdit(int id = 0)
        {
            ViewData["ActivePage"] = "Product";
            await PopulateDropDownsAsync();

            if (id == 0)
            {
                return View(new ProductViewModel());
            }

            var viewModel = await _productService.GetProductViewModelAsync(id);
            if (viewModel == null)
            {
                return NotFound();
            }

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOrEdit(ProductViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateDropDownsAsync(model.CategoryId, model.SupplierId, model.BaseUnitId);
                return View(model);
            }

            var (success, errorMessage, _) = await _productService.CreateOrUpdateProductAsync(model);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, errorMessage ?? "Error saving product.");
                await PopulateDropDownsAsync(model.CategoryId, model.SupplierId, model.BaseUnitId);
                return View(model);
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateDropDownsAsync(int? selectedCategoryId = null, int? selectedSupplierId = null, int? selectedBaseUnitId = null)
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            var suppliers = await _supplierService.GetActiveSuppliersAsync();
            var units = await _unitService.GetAllUnitsAsync();

            ViewBag.CategoryId = new SelectList(categories, "Id", "Name", selectedCategoryId);
            ViewBag.SupplierId = new SelectList(suppliers, "Id", "Name", selectedSupplierId);
            ViewBag.BaseUnitId = new SelectList(units, "Id", "Name", selectedBaseUnitId);
        }
    }
}
