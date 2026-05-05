using ims_inv.Data;
using ims_inv.Models;
using ims_inv.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ims_inv.Controllers
{
    public class StockMovementController : Controller
    {
        private readonly WebAppDbContext _db;
        private readonly UnitConversionService _unitConversionService;

        public StockMovementController(WebAppDbContext db, UnitConversionService unitConversionService)
        {
            _db = db;
            _unitConversionService = unitConversionService;
        }

        // View Movement History
        public async Task<IActionResult> Index()
        {
            ViewData["ActivePage"] = "StockEntry";
            var stocks = await _db.StockMovements
                .Include(p => p.Product)
                .Include(w => w.Warehouse)
                .ToListAsync();
            return View(new List<StockMovement>(stocks));
        }

        // New Stock Entry (IN/OUT)
        public async Task<IActionResult> Create()
        {
            ViewData["ActivePage"] = "StockEntry";
            var products = await _db.Products.Include(p => p.Unit).ToListAsync();
            var wareHouse = await _db.Warehouses.ToListAsync();
            var units = await _db.Units.ToListAsync();
            ViewBag.Products = products;
            ViewBag.productId = new SelectList(products, "Id", "Name");
            ViewBag.warehouseId = new SelectList(wareHouse, "Id", "Name");
            ViewBag.units = new SelectList(units, "Id", "Name");
            return View(new CreateStockMovementViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(CreateStockMovementViewModel stockMovementView)
        {

            ViewData["ActivePage"] = "StockEntry";
            if (!ModelState.IsValid)
            {
                var products = await _db.Products.Include(p => p.Unit).ToListAsync();
                var wareHouse = await _db.Warehouses.ToListAsync();
                var units = await _db.Units.ToListAsync();
                ViewBag.productId = new SelectList(products, "Id", "Name", stockMovementView.ProductId);
                ViewBag.warehouseId = new SelectList(wareHouse, "Id", "Name", stockMovementView.WarehouseId);
                ViewBag.units = new SelectList(units, "Id", "Name", stockMovementView.EntryUnitId);
                return View("create", stockMovementView);
            }

            try
            {
                var product = await _db.Products.FindAsync(stockMovementView.ProductId);
                if (product == null)
                {
                    ModelState.AddModelError("", "Product not found");
                    return RedirectToAction("Create");
                }

                // Determine the quantity to store in base unit
                decimal quantityInBaseUnit = stockMovementView.Quantity;

                // If user entered quantity in a different unit, convert it
                if (stockMovementView.EntryUnitId.HasValue && stockMovementView.EntryUnitId != product.BaseUnitId)
                {

                    quantityInBaseUnit = await _unitConversionService.ConvertAsync(
                        stockMovementView.ProductId,
                        stockMovementView.Quantity,
                        stockMovementView.EntryUnitId.Value,
                        product.BaseUnitId
                    );
                }

                var stockMovement = new StockMovement
                {
                    ProductId = stockMovementView.ProductId,
                    WarehouseId = stockMovementView.WarehouseId,
                    MovementType = stockMovementView.MovementType,
                    Quantity = (int)quantityInBaseUnit,  // Store in base unit
                    ReferenceNumber = stockMovementView.ReferenceNumber,
                    Reason = stockMovementView.Reason,
                    Notes = stockMovementView.Notes,
                };

                // Update inventory
                var inventory = await _db.Inventories.Where(p => p.ProductId == stockMovementView.ProductId).FirstOrDefaultAsync();
                if (inventory == null)
                {
                    var inventoryEntry = new Inventory
                    {
                        ProductId = stockMovementView.ProductId,
                        Quantity = (int)quantityInBaseUnit,
                        LastCountedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now,
                    };
                    await _db.Inventories.AddAsync(inventoryEntry);
                }
                else
                {
                    if (stockMovementView.MovementType == "IN")
                        inventory.Quantity += (int)quantityInBaseUnit;
                    else if (stockMovementView.MovementType == "OUT")
                        inventory.Quantity -= (int)quantityInBaseUnit;

                    inventory.LastCountedAt = DateTime.Now;
                    inventory.UpdatedAt = DateTime.Now;
                    _db.Inventories.Update(inventory);
                }

                await _db.StockMovements.AddAsync(stockMovement);
                await _db.SaveChangesAsync();

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                ModelState.AddModelError("", $"Error: {ex.Message}");
                var products = await _db.Products.Include(p => p.Unit).ToListAsync();
                var wareHouse = await _db.Warehouses.ToListAsync();
                var units = await _db.Units.ToListAsync();
                ViewBag.productId = new SelectList(products, "Id", "Name", stockMovementView.ProductId);
                ViewBag.warehouseId = new SelectList(wareHouse, "Id", "Name", stockMovementView.WarehouseId);
                ViewBag.units = new SelectList(units, "Id", "Name", stockMovementView.EntryUnitId);
                return View("create", stockMovementView);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetUnitConversionsByProductId(int productId)
        {
            var baseUnit = await _db.Products
                .Where(p => p.Id == productId)
                .Select(p => p.Unit.Name)
                .FirstOrDefaultAsync();

            var units = await _db.UnitConversions
                .Where(x => x.ProductId == productId)
                .Select(x => new
                {
                    unitId = x.ToUnitId,
                    unitName = x.ToUnit.Name,
                    factor = x.ConversionFactor
                })
                .ToListAsync();

            return Ok(new
            {
                success = true,
                baseUnit,
                data = units
            });
        }
    }
}
