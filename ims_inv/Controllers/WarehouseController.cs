using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ims_inv.Data;
using ims_inv.Models;

namespace ims_inv.Controllers
{
    [Authorize]
    public class WarehouseController(WebAppDbContext _dbContext) : Controller
    {
        public async Task<IActionResult> Index()
        {
            ViewData["ActivePage"] = "Warehouse";
            var warehouses = await _dbContext.Warehouses.ToListAsync();
            return View(warehouses);
        }

        [HttpGet]
        public async Task<IActionResult> CreateOrEdit(int id = 0)
        {
            ViewData["ActivePage"] = "Warehouse";
            if (id == 0)
            {
                return View(new WarehouseViewModel());
            }

            var warehouse = await _dbContext.Warehouses.FindAsync(id);
            if (warehouse == null)
            {
                return NotFound();
            }

            var viewModel = new WarehouseViewModel
            {
                Id = warehouse.Id,
                Name = warehouse.Name,
                Address = warehouse.Address,
                Phone = warehouse.Phone,
                Email = warehouse.Email,
                IsActive = warehouse.IsActive,
                Capacity = warehouse.Capacity
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOrEdit(WarehouseViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (model.Id == 0)
            {
                var warehouse = new Warehouse
                {
                    Name = model.Name,
                    Address = model.Address,
                    Phone = model.Phone,
                    Email = model.Email,
                    IsActive = model.IsActive,
                    Capacity = model.Capacity
                };
                _dbContext.Add(warehouse);
            }
            else
            {
                var warehouse = await _dbContext.Warehouses.FindAsync(model.Id);
                if (warehouse == null)
                {
                    return NotFound();
                }

                warehouse.Name = model.Name;
                warehouse.Address = model.Address;
                warehouse.Phone = model.Phone;
                warehouse.Email = model.Email;
                warehouse.IsActive = model.IsActive;
                warehouse.Capacity = model.Capacity;

                _dbContext.Update(warehouse);
            }

            await _dbContext.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var warehouse = await _dbContext.Warehouses.FindAsync(id);
            if (warehouse != null)
            {
                _dbContext.Warehouses.Remove(warehouse);
                await _dbContext.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
