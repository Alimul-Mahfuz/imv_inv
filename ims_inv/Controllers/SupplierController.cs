using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ims_inv.Data;
using ims_inv.Models;

namespace ims_inv.Controllers
{
    [Authorize]
    public class SupplierController(WebAppDbContext _dbContext) : Controller
    {
        public async Task<IActionResult> Index()
        {
            ViewData["ActivePage"] = "Supplier";
            var suppliers = await _dbContext.Suppliers.ToListAsync();
            return View(suppliers);
        }

        [HttpGet]
        public async Task<IActionResult> CreateOrEdit(int id = 0)
        {
            ViewData["ActivePage"] = "Supplier";
            if (id == 0)
            {
                return View(new SupplierViewModel());
            }

            var supplier = await _dbContext.Suppliers.FindAsync(id);
            if (supplier == null)
            {
                return NotFound();
            }

            var viewModel = new SupplierViewModel
            {
                Id = supplier.Id,
                Name = supplier.Name,
                ContactPerson = supplier.ContactPerson,
                Address = supplier.Address,
                Phone = supplier.Phone,
                Email = supplier.Email,
                IsActive = supplier.IsActive
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOrEdit(SupplierViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (model.Id == 0)
            {
                var supplier = new Supplier
                {
                    Name = model.Name,
                    ContactPerson = model.ContactPerson,
                    Address = model.Address,
                    Phone = model.Phone,
                    Email = model.Email,
                    IsActive = model.IsActive
                };
                _dbContext.Add(supplier);
            }
            else
            {
                var supplier = await _dbContext.Suppliers.FindAsync(model.Id);
                if (supplier == null)
                {
                    return NotFound();
                }

                supplier.Name = model.Name;
                supplier.ContactPerson = model.ContactPerson;
                supplier.Address = model.Address;
                supplier.Phone = model.Phone;
                supplier.Email = model.Email;
                supplier.IsActive = model.IsActive;

                _dbContext.Update(supplier);
            }

            await _dbContext.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var supplier = await _dbContext.Suppliers.FindAsync(id);
            if (supplier != null)
            {
                _dbContext.Suppliers.Remove(supplier);
                await _dbContext.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
