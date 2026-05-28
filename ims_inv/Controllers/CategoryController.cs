using ims_inv.Data;
using ims_inv.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ims_inv.Controllers
{
    [Authorize]
    public class CategoryController(WebAppDbContext _dbContext) : Controller
    {
        public async Task<IActionResult> Index()
        {
            ViewData["ActivePage"] = "Category";
            var categories = await _dbContext.Categories
                .Include(c => c.Parent)
                .ToListAsync();
            return View(categories);
        }

        [HttpGet]
        public async Task<IActionResult> CreateOrEdit(int id = 0)
        {
            ViewData["ActivePage"] = "Category";

            var categories = await _dbContext.Categories
                .Where(c => c.Id != id)
                .ToListAsync();
            ViewBag.ParentId = new SelectList(categories, "Id", "Name");

            if (id == 0)
            {
                return View(new CategoryViewModel());
            }

            var category = await _dbContext.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            var viewModel = new CategoryViewModel
            {
                Id = category.Id,
                Name = category.Name,
                ParentId = category.ParentId
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOrEdit(CategoryViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var categories = await _dbContext.Categories.Where(c => c.Id != model.Id).ToListAsync();
                ViewBag.ParentId = new SelectList(categories, "Id", "Name", model.ParentId);
                return View(model);
            }

            if (model.Id == 0)
            {
                var category = new Category
                {
                    Name = model.Name,
                    ParentId = model.ParentId,
                    CreatedAt = DateTime.UtcNow
                };
                _dbContext.Add(category);
            }
            else
            {
                var category = await _dbContext.Categories.FindAsync(model.Id);
                if (category == null)
                {
                    return NotFound();
                }

                category.Name = model.Name;
                category.ParentId = model.ParentId;

                _dbContext.Update(category);
            }

            await _dbContext.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _dbContext.Categories
                .Include(c => c.Children)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category != null)
            {
                if (category.Children.Any())
                {
                    TempData["Error"] = "Cannot delete category that has sub-categories.";
                    return RedirectToAction(nameof(Index));
                }

                _dbContext.Categories.Remove(category);
                await _dbContext.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
