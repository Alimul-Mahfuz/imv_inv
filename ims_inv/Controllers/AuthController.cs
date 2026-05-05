using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using ims_inv.Controllers.Filters;
using ims_inv.Data;
using ims_inv.Models;

namespace ims_inv.Controllers
{
    public class AuthController(WebAppDbContext _dbContext) : Controller
    {
        [HttpGet]
        [RedirectIfLoggedInFilter]
        public async Task<IActionResult> Login()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> DoLogin(LoginViewModel loginView)
        {
            if (!ModelState.IsValid)
            {
                return View("Login", loginView);
            }

            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == loginView.Email);
            if (user == null)
            {
                ModelState.AddModelError("email", "Credentials not found");
                return View("Login", loginView);
            }
            var hasher = new PasswordHasher<User>();
            var result = hasher.VerifyHashedPassword(user, user.Password, loginView.Password);
            if (result == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError("password", "Invalid credentials");
                return View("Login", loginView);
            }
            var claim = new List<Claim>()
            {
                new Claim(ClaimTypes.Name,user.Name),
                new Claim(ClaimTypes.Email,user.Email),
                new Claim(ClaimTypes.NameIdentifier,user.Id.ToString())
            };

            var idenity = new ClaimsIdentity(claim, "CookieAuth");
            var principle = new ClaimsPrincipal(idenity);

            await HttpContext.SignInAsync(principle);
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction("Login", "Auth");
        }
    }
}
