using ims_inv.Data;
using ims_inv.Models;
using ims_inv.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace ims_inv.Controllers
{
    public class UnitConversionController : Controller
    {
        private readonly WebAppDbContext _db;
        private readonly UnitConversionService _unitConversionService;

        public UnitConversionController(WebAppDbContext db, UnitConversionService unitConversionService)
        {
            _db = db;
            _unitConversionService = unitConversionService;
        }

        // GET: UnitConversion/Product/{productId}
        public async Task<IActionResult> ProductConversions(int productId)
        {
            ViewData["ActivePage"] = "Products";

            var product = await _db.Products
                .Include(p => p.Unit)
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null)
                return NotFound();

            var conversions = await _unitConversionService.GetConversionsForProductAsync(productId);

            var model = new ProductUnitConversionViewModel
            {
                ProductId = product.Id,
                ProductName = product.Name,
                ProductSKU = product.SKU,
                BaseUnitId = product.BaseUnitId,
                BaseUnitName = product.Unit.Name,
                BaseUnitSymbol = product.Unit.Symbol,
                Conversions = conversions.Select(c => new UnitConversionViewModel
                {
                    Id = c.Id,
                    FromUnitId = c.FromUnitId,
                    FromUnitName = c.FromUnit.Name,
                    FromUnitSymbol = c.FromUnit.Symbol,
                    ToUnitId = c.ToUnitId,
                    ToUnitName = c.ToUnit.Name,
                    ToUnitSymbol = c.ToUnit.Symbol,
                    ConversionFactor = c.ConversionFactor
                }).ToList(),
                AllUnits = await _db.Units.ToListAsync()
            };

            return View(model);
        }

        // GET: UnitConversion/Create/{productId}
        public async Task<IActionResult> Create(int productId)
        {
            ViewData["ActivePage"] = "Products";

            var product = await _db.Products
                .Include(p => p.Unit)
                .FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null)
                return NotFound();

            var model = new CreateUnitConversionViewModel
            {
                ProductId = productId,
                ProductName = product.Name,
                BaseUnitId = product.BaseUnitId,
                BaseUnitName = product.Unit.Name,
                AvailableUnits = await _db.Units.ToListAsync()
            };

            return View(model);
        }

        // POST: UnitConversion/Create/{productId}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int productId, CreateUnitConversionViewModel model)
        {
            ViewData["ActivePage"] = "Products";

            if (!ModelState.IsValid)
            {
                model.AvailableUnits = await _db.Units.ToListAsync();
                return View(model);
            }

            var product = await _db.Products
                .Include(p => p.Unit)
                .FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null)
                return NotFound();

            try
            {
                // Determine which unit is FROM and which is TO
                int fromUnitId = product.BaseUnitId;
                int toUnitId = model.TargetUnitId;

                // Check if conversion already exists
                var existingConversion = await _db.UnitConversions
                    .FirstOrDefaultAsync(uc => uc.ProductId == productId && uc.FromUnitId == fromUnitId && uc.ToUnitId == toUnitId);

                if (existingConversion != null)
                {
                    ModelState.AddModelError("", "A conversion from this unit to the target unit already exists for this product.");
                    model.AvailableUnits = await _db.Units.ToListAsync();
                    return View(model);
                }

                // Create the conversion
                await _unitConversionService.CreateConversionAsync(
                    productId,
                    fromUnitId,
                    toUnitId,
                    model.ConversionFactor);

                TempData["SuccessMessage"] = $"Unit conversion created successfully!";
                return RedirectToAction("ProductConversions", new { productId });
            }
            catch (Exception ex)
            {

                ModelState.AddModelError("", $"Error: {ex.Message}");
                model.AvailableUnits = await _db.Units.ToListAsync();
                return View(model);
            }
        }

        // GET: UnitConversion/Edit/{conversionId}
        public async Task<IActionResult> Edit(int conversionId, int productId)
        {
            ViewData["ActivePage"] = "Products";

            var conversion = await _db.UnitConversions
                .Include(uc => uc.FromUnit)
                .Include(uc => uc.ToUnit)
                .FirstOrDefaultAsync(uc => uc.Id == conversionId);

            if (conversion == null)
                return NotFound();

            var product = await _db.Products.FindAsync(productId);
            if (product == null)
                return NotFound();

            var model = new EditUnitConversionViewModel
            {
                ConversionId = conversion.Id,
                ProductId = productId,
                FromUnitName = conversion.FromUnit.Name,
                FromUnitSymbol = conversion.FromUnit.Symbol,
                ToUnitName = conversion.ToUnit.Name,
                ToUnitSymbol = conversion.ToUnit.Symbol,
                ConversionFactor = conversion.ConversionFactor
            };

            return View(model);
        }

        // POST: UnitConversion/Edit/{conversionId}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int conversionId, int productId, EditUnitConversionViewModel model)
        {
            ViewData["ActivePage"] = "Products";

            if (!ModelState.IsValid)
                return View(model);

            try
            {
                await _unitConversionService.UpdateConversionAsync(conversionId, model.ConversionFactor);

                TempData["SuccessMessage"] = "Unit conversion updated successfully!";
                return RedirectToAction("ProductConversions", new { productId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error: {ex.Message}");
                return View(model);
            }
        }

        // POST: UnitConversion/Delete/{conversionId}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int conversionId, int productId)
        {
            try
            {
                await _unitConversionService.DeleteConversionAsync(conversionId);

                TempData["SuccessMessage"] = "Unit conversion deleted successfully!";
                return RedirectToAction("ProductConversions", new { productId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error: {ex.Message}";
                return RedirectToAction("ProductConversions", new { productId });
            }
        }
    }

    // ============================================================================
    // VIEW MODELS
    // ============================================================================

    public class ProductUnitConversionViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductSKU { get; set; }
        public int BaseUnitId { get; set; }
        public string BaseUnitName { get; set; }
        public string BaseUnitSymbol { get; set; }
        public List<UnitConversionViewModel> Conversions { get; set; } = new();
        public List<Unit> AllUnits { get; set; } = new();
    }

    public class CreateUnitConversionViewModel
    {
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public int BaseUnitId { get; set; }
        public string? BaseUnitName { get; set; }

        [Required(ErrorMessage = "Target unit is required")]
        [Display(Name = "Convert To")]
        public int TargetUnitId { get; set; }

        [Required(ErrorMessage = "Conversion factor is required")]
        [Range(0.0001, 999999.9999, ErrorMessage = "Conversion factor must be between 0.0001 and 999999.9999")]
        [Display(Name = "Conversion Factor")]
        public decimal ConversionFactor { get; set; }

        public List<Unit> AvailableUnits { get; set; } = new();
    }

    public class EditUnitConversionViewModel
    {
        public int ConversionId { get; set; }
        public int ProductId { get; set; }
        public string FromUnitName { get; set; }
        public string FromUnitSymbol { get; set; }
        public string ToUnitName { get; set; }
        public string ToUnitSymbol { get; set; }

        [Required(ErrorMessage = "Conversion factor is required")]
        [Range(0.0001, 999999.9999, ErrorMessage = "Conversion factor must be between 0.0001 and 999999.9999")]
        [Display(Name = "Conversion Factor")]
        public decimal ConversionFactor { get; set; }
    }
}
