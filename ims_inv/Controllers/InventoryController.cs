using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ims_inv.Helper;
using ims_inv.Models;
using ims_inv.Services;

namespace ims_inv.Controllers
{
    [Authorize]
    public class InventoryController : Controller
    {
        private readonly IInventoryService _inventoryService;
        private readonly IWarehouseService _warehouseService;
        private readonly IProductService _productService;
        private readonly IUnitService _unitService;
        private readonly AuthUser _authUser;

        public InventoryController(
            IInventoryService inventoryService,
            IWarehouseService warehouseService,
            IProductService productService,
            IUnitService unitService,
            AuthUser authUser)
        {
            _inventoryService = inventoryService;
            _warehouseService = warehouseService;
            _productService = productService;
            _unitService = unitService;
            _authUser = authUser;
        }

        public async Task<IActionResult> Index(int? warehouseId = null)
        {
            ViewData["ActivePage"] = "Inventory";
            var warehouses = await _warehouseService.GetAllWarehousesAsync();
            ViewBag.Warehouses = new SelectList(warehouses, "Id", "Name", warehouseId);
            ViewBag.SelectedWarehouseId = warehouseId;

            var inventory = await _inventoryService.GetAllInventoryAsync(warehouseId);
            return View(inventory);
        }

        // GET: /Inventory/Transfer
        public async Task<IActionResult> Transfer(int? productId = null, int? fromWarehouseId = null)
        {
            ViewData["ActivePage"] = "Inventory";
            await PopulateTransferDropDownsAsync(productId, fromWarehouseId);
            return View(new StockTransferViewModel
            {
                ProductId = productId ?? 0,
                FromWarehouseId = fromWarehouseId ?? 0
            });
        }

        // POST: /Inventory/Transfer
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Transfer(StockTransferViewModel model)
        {
            ViewData["ActivePage"] = "Inventory";
            if (!ModelState.IsValid)
            {
                await PopulateTransferDropDownsAsync(model.ProductId, model.FromWarehouseId, model.ToWarehouseId, model.EntryUnitId);
                return View(model);
            }

            var (success, errorMessage) = await _inventoryService.TransferStockAsync(model, _authUser.Id);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, errorMessage ?? "Error processing stock transfer.");
                await PopulateTransferDropDownsAsync(model.ProductId, model.FromWarehouseId, model.ToWarehouseId, model.EntryUnitId);
                return View(model);
            }

            TempData["SuccessMessage"] = "Stock transferred successfully.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Inventory/Adjustment
        public async Task<IActionResult> Adjustment(int? productId = null, int? warehouseId = null)
        {
            ViewData["ActivePage"] = "Inventory";
            await PopulateAdjustmentDropDownsAsync(productId, warehouseId);

            decimal currentQty = 0m;
            if (productId.HasValue && warehouseId.HasValue)
            {
                var inv = await _inventoryService.GetInventoryByProductAndWarehouseAsync(productId.Value, warehouseId.Value);
                currentQty = inv?.Quantity ?? 0m;
            }

            return View(new StockAdjustmentViewModel
            {
                ProductId = productId ?? 0,
                WarehouseId = warehouseId ?? 0,
                CurrentSystemQuantity = currentQty,
                PhysicalCount = currentQty
            });
        }

        // POST: /Inventory/Adjustment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Adjustment(StockAdjustmentViewModel model)
        {
            ViewData["ActivePage"] = "Inventory";
            if (!ModelState.IsValid)
            {
                await PopulateAdjustmentDropDownsAsync(model.ProductId, model.WarehouseId);
                return View(model);
            }

            var (success, errorMessage) = await _inventoryService.AdjustStockAsync(model, _authUser.Id);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, errorMessage ?? "Error recording stock adjustment.");
                await PopulateAdjustmentDropDownsAsync(model.ProductId, model.WarehouseId);
                return View(model);
            }

            TempData["SuccessMessage"] = "Stock adjusted successfully.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Inventory/StockCard?productId=X
        public async Task<IActionResult> StockCard(int productId, int? warehouseId = null)
        {
            ViewData["ActivePage"] = "Inventory";
            var model = await _inventoryService.GetStockCardAsync(productId, warehouseId);
            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }

        // GET: /Inventory/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            ViewData["ActivePage"] = "Inventory";
            var item = await _inventoryService.GetInventoryByIdAsync(id);
            if (item == null)
            {
                return NotFound();
            }

            var model = new EditReorderSettingsViewModel
            {
                Id = item.Id,
                ProductId = item.ProductId,
                ProductName = item.Product?.Name ?? "N/A",
                ProductSKU = item.Product?.SKU ?? "N/A",
                WarehouseId = item.WarehouseId,
                WarehouseName = item.Warehouse?.Name ?? "N/A",
                UnitSymbol = item.Product?.Unit?.Symbol ?? "units",
                CurrentQuantity = item.Quantity,
                ReorderLevel = item.ReorderLevel,
                ReorderQuantity = item.ReorderQuantity
            };

            return View(model);
        }

        // POST: /Inventory/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditReorderSettingsViewModel model)
        {
            ViewData["ActivePage"] = "Inventory";
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var success = await _inventoryService.UpdateInventoryLevelsAsync(model.Id, model.ReorderLevel, model.ReorderQuantity);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, "Inventory record not found.");
                return View(model);
            }

            TempData["SuccessMessage"] = "Reorder levels updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        // AJAX API: Get Current Stock for Product in Warehouse
        [HttpGet]
        public async Task<IActionResult> GetCurrentStock(int productId, int warehouseId)
        {
            var inv = await _inventoryService.GetInventoryByProductAndWarehouseAsync(productId, warehouseId);
            var product = await _productService.GetProductByIdAsync(productId);
            return Ok(new
            {
                success = true,
                quantity = inv?.Quantity ?? 0m,
                unitSymbol = product?.Unit?.Symbol ?? "units"
            });
        }

        private async Task PopulateTransferDropDownsAsync(int? productId = null, int? fromWarehouseId = null, int? toWarehouseId = null, int? unitId = null)
        {
            var products = await _productService.GetAllProductsAsync();
            var warehouses = await _warehouseService.GetActiveWarehousesAsync();
            var units = await _unitService.GetAllUnitsAsync();

            ViewBag.Products = new SelectList(products, "Id", "Name", productId);
            ViewBag.FromWarehouses = new SelectList(warehouses, "Id", "Name", fromWarehouseId);
            ViewBag.ToWarehouses = new SelectList(warehouses, "Id", "Name", toWarehouseId);
            ViewBag.Units = new SelectList(units, "Id", "Name", unitId);
        }

        private async Task PopulateAdjustmentDropDownsAsync(int? productId = null, int? warehouseId = null)
        {
            var products = await _productService.GetAllProductsAsync();
            var warehouses = await _warehouseService.GetActiveWarehousesAsync();

            ViewBag.Products = new SelectList(products, "Id", "Name", productId);
            ViewBag.Warehouses = new SelectList(warehouses, "Id", "Name", warehouseId);
        }
    }
}
