using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using ims_inv.Controllers.Filters;
using ims_inv.Models;
using ims_inv.Services;

namespace ims_inv.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpGet]
        [RedirectIfLoggedInFilter]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DoLogin(LoginViewModel loginView)
        {
            if (!ModelState.IsValid)
            {
                return View("Login", loginView);
            }

            var (success, errorMessage, principal) = await _authService.ValidateLoginAsync(loginView.Email, loginView.Password);
            if (!success || principal == null)
            {
                ModelState.AddModelError(string.Empty, errorMessage ?? "Invalid credentials");
                return View("Login", loginView);
            }

            await HttpContext.SignInAsync(principal);
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction("Login", "Auth");
        }
    }
}
