using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ims_inv.Helper;
using ims_inv.Models;
using ims_inv.Services;

namespace ims_inv.Controllers
{
    [Authorize]
    public class StockMovementController : Controller
    {
        private readonly IStockMovementService _stockMovementService;
        private readonly IProductService _productService;
        private readonly IWarehouseService _warehouseService;
        private readonly IUnitService _unitService;
        private readonly IUnitConversionService _unitConversionService;
        private readonly AuthUser _authUser;

        public StockMovementController(
            IStockMovementService stockMovementService,
            IProductService productService,
            IWarehouseService warehouseService,
            IUnitService unitService,
            IUnitConversionService unitConversionService,
            AuthUser authUser)
        {
            _stockMovementService = stockMovementService;
            _productService = productService;
            _warehouseService = warehouseService;
            _unitService = unitService;
            _unitConversionService = unitConversionService;
            _authUser = authUser;
        }

        // View Movement History
        public async Task<IActionResult> Index()
        {
            ViewData["ActivePage"] = "StockEntry";
            var stocks = await _stockMovementService.GetAllMovementsAsync();
            return View(stocks);
        }

        // New Stock Entry (IN/OUT)
        public async Task<IActionResult> Create()
        {
            ViewData["ActivePage"] = "StockEntry";
            await PopulateDropDownsAsync();
            return View(new CreateStockMovementViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(CreateStockMovementViewModel stockMovementView)
        {
            ViewData["ActivePage"] = "StockEntry";
            if (!ModelState.IsValid)
            {
                await PopulateDropDownsAsync(stockMovementView.ProductId, stockMovementView.WarehouseId, stockMovementView.EntryUnitId);
                return View("Create", stockMovementView);
            }

            var (success, errorMessage, _) = await _stockMovementService.RecordStockMovementAsync(stockMovementView, _authUser.Id);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, errorMessage ?? "Error recording stock movement.");
                await PopulateDropDownsAsync(stockMovementView.ProductId, stockMovementView.WarehouseId, stockMovementView.EntryUnitId);
                return View("Create", stockMovementView);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> GetUnitConversionsByProductId(int productId)
        {
            var product = await _productService.GetProductByIdAsync(productId);
            var conversions = await _unitConversionService.GetConversionsForProductAsync(productId);

            var units = conversions.Select(x => new
            {
                unitId = x.ToUnitId,
                unitName = x.ToUnit?.Name,
                factor = x.ConversionFactor
            }).ToList();

            return Ok(new
            {
                success = true,
                baseUnit = product?.Unit?.Name,
                data = units
            });
        }

        private async Task PopulateDropDownsAsync(int? selectedProductId = null, int? selectedWarehouseId = null, int? selectedUnitId = null)
        {
            var products = await _productService.GetAllProductsAsync();
            var warehouses = await _warehouseService.GetActiveWarehousesAsync();
            var units = await _unitService.GetAllUnitsAsync();

            ViewBag.Products = products;
            ViewBag.productId = new SelectList(products, "Id", "Name", selectedProductId);
            ViewBag.warehouseId = new SelectList(warehouses, "Id", "Name", selectedWarehouseId);
            ViewBag.units = new SelectList(units, "Id", "Name", selectedUnitId);
        }
    }
}
