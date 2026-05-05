using ims_inv.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ims_inv.Controllers
{
    public class InventoryController(WebAppDbContext _db) : Controller
    {
        public async Task<IActionResult> Index()
        {
            ViewData["ActivePage"] = "Inventory";
            var inventory = await _db.Inventories.Include(p => p.Product).ToListAsync();
            return View(inventory);
        }
    }
}
