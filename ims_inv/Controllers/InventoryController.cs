using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ims_inv.Models;
using ims_inv.Services;

namespace ims_inv.Controllers
{
    [Authorize]
    public class InventoryController : Controller
    {
        private readonly IInventoryService _inventoryService;
        private readonly IWarehouseService _warehouseService;

        public InventoryController(
            IInventoryService inventoryService,
            IWarehouseService warehouseService)
        {
            _inventoryService = inventoryService;
            _warehouseService = warehouseService;
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
    }
}
