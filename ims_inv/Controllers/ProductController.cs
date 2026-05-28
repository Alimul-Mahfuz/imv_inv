using ims_inv.Data;
using ims_inv.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ims_inv.Controllers
{
    public class ProductController(WebAppDbContext _db) : Controller
    {
        public async Task<IActionResult> Index()
        {
            var product = await _db.Products
                .Include(c => c.Category)
                .Include(s => s.Supplier)
                .Include(u => u.Unit)
                .ToListAsync();
            return View(product);
        }

        public async Task<IActionResult> CreateOrEdit(int Id = 0)
        {
            ViewData["ActivePage"] = "Product";
            var categories = await _db.Categories.ToListAsync();
            var suppliers = await _db.Suppliers.ToListAsync();
            var units = await _db.Units.ToListAsync();
            ViewBag.CategoryId = new SelectList(categories, "Id", "Name");
            ViewBag.SupplierId = new SelectList(suppliers, "Id", "Name");
            ViewBag.BaseUnitId = new SelectList(units, "Id", "Name");
            if (Id == 0)
            {
                return View(new ProductViewModel());
            }

            var product = _db.Products
                .FirstOrDefault(x => x.Id == Id);
            if (product == null)
            {
                return NotFound();
            }

            var productViewModel = new ProductViewModel
            {
                Id = product.Id,
                Name = product.Name,
                SKU = product.SKU,
                CategoryId = product.CategoryId,
                SupplierId = product.SupplierId,
                BaseUnitId = product.BaseUnitId,
            };

            return View(productViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOrEdit(ProductViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var categories = await _db.Categories.ToListAsync();
                var suppliers = await _db.Suppliers.ToListAsync();
                var units = await _db.Units.ToListAsync();
                ViewBag.CategoryId = new SelectList(categories, "Id", "Name");
                ViewBag.SupplierId = new SelectList(suppliers, "Id", "Name");
                ViewBag.BaseUnitId = new SelectList(units, "Id", "Name");

                return View(model);
            }

            if (model.Id == 0)
            {
                var isSKUExist = await _db.Products.AnyAsync(p => p.SKU == model.SKU);
                if (isSKUExist)
                {
                    ModelState.AddModelError("SKU", "SKU is exists");
                    return View(model);
                }
                var product = new Product
                {
                    Id = model.Id,
                    SKU = model.SKU,
                    Name = model.Name,
                    CategoryId = model.CategoryId,
                    SupplierId = model.SupplierId,
                    BaseUnitId = model.BaseUnitId,
                    CreatedAt = DateTime.UtcNow

                };

                await _db.Products.AddAsync(product);
            }
            else
            {
                var product = await _db.Products.Where(p => p.Id == model.Id).FirstOrDefaultAsync();
                if (product == null)
                {
                    return NotFound();
                }

                var isSKUExist = await _db.Products.AnyAsync(p => p.SKU == model.SKU && p.Id != product.Id);
                if (isSKUExist)
                {
                    ModelState.AddModelError("SKU", "SKU is exists");
                    return View(model);
                }

                product.Name = model.Name;
                product.SKU = model.SKU;
                product.BaseUnitId = model.BaseUnitId;
                product.CategoryId = model.CategoryId;
                product.SupplierId = model.SupplierId;

                _db.Products.Update(product);


            }


            await _db.SaveChangesAsync();

            return RedirectToAction("Index");
        }
    }
}
