using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ims_inv.Data;
using ims_inv.Models;

namespace ims_inv.Controllers
{
    [Authorize]
    public class UnitController(WebAppDbContext _dbContext) : Controller
    {
        public async Task<IActionResult> Index()
        {
            ViewData["ActivePage"] = "Unit";
            var units = await _dbContext.Units.ToListAsync();
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

            var unit = await _dbContext.Units.FindAsync(id);
            if (unit == null)
            {
                return NotFound();
            }

            var viewModel = new UnitViewModel
            {
                Id = unit.Id,
                Name = unit.Name,
                Symbol = unit.Symbol,
                Type = unit.Type
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOrEdit(UnitViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (model.Id == 0)
            {
                var unit = new Unit
                {
                    Name = model.Name,
                    Symbol = model.Symbol,
                    Type = model.Type
                };
                _dbContext.Add(unit);
            }
            else
            {
                var unit = await _dbContext.Units.FindAsync(model.Id);
                if (unit == null)
                {
                    return NotFound();
                }

                unit.Name = model.Name;
                unit.Symbol = model.Symbol;
                unit.Type = model.Type;

                _dbContext.Update(unit);
            }

            await _dbContext.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var unit = await _dbContext.Units.FindAsync(id);
            if (unit != null)
            {
                _dbContext.Units.Remove(unit);
                await _dbContext.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
