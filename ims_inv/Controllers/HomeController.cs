using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ims_inv.Models;
using ims_inv.Services;

namespace ims_inv.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IDashboardService _dashboardService;

        public HomeController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        public IActionResult Index()
        {
            ViewData["ActivePage"] = "Dashboard";
            return View();
        }

        public IActionResult Privacy()
        {
            ViewData["ActivePage"] = "Privacy";
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
