// ============================================================================
// CONTROLLER EXAMPLE: Using Domain Events with User Registration
// ============================================================================
// File: Controllers/AuthController.cs

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebApplication1.Controllers.Filters;
using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.Events;  // ✅ ADD THIS IMPORT

namespace WebApplication1.Controllers
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
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            };

            var idenity = new ClaimsIdentity(claim, "CookieAuth");
            var principle = new ClaimsPrincipal(idenity);

            await HttpContext.SignInAsync(principle);
            return RedirectToAction("Index", "Home");
        }

        // ============================================================================
        // ✅ NEW: Registration endpoints using UserRegisteredEvent
        // ============================================================================

        [HttpGet]
        [RedirectIfLoggedInFilter]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> DoRegister(CreateUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Register", model);
            }

            // Check if user already exists
            var existingUser = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Email == model.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError("email", "Email is already registered");
                return View("Register", model);
            }

            // Hash password
            var hasher = new PasswordHasher<User>();
            var hashedPassword = hasher.HashPassword(null, model.Password);

            // Create new user
            var newUser = new User
            {
                Name = model.Name,
                Email = model.Email,
                Password = hashedPassword
            };

            // ✅ STEP 1: Add domain event to the user entity
            newUser.AddDomainEvent(new UserRegisteredEvent(newUser));

            // ✅ STEP 2: Add user to database
            _dbContext.Users.Add(newUser);

            // ✅ STEP 3: Save changes (CRITICAL - this triggers the interceptor)
            await _dbContext.SaveChangesAsync();
            // 
            // After SaveChangesAsync():
            // 1. Database commit succeeds
            // 2. DomainEventsInterceptor.SavedChangesAsync() is triggered
            // 3. Interceptor finds UserRegisteredEvent
            // 4. Interceptor finds UserObserver handler
            // 5. UserObserver.Handle() executes
            // 6. UserObserver sends welcome email
            // 7. Event is cleared from the entity

            // Optional: Auto-login after registration
            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, newUser.Name),
                new Claim(ClaimTypes.Email, newUser.Email),
                new Claim(ClaimTypes.NameIdentifier, newUser.Id.ToString())
            };

            var identity = new ClaimsIdentity(claims, "CookieAuth");
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(principal);

            // Redirect to home
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

// ============================================================================
// EXPLANATION OF WHAT HAPPENS:
// ============================================================================
/*
 * FLOW WHEN USER REGISTERS:
 * 
 * 1. User submits registration form (name, email, password)
 *    ↓
 * 2. DoRegister() action is called
 *    ↓
 * 3. Validation checks
 *    - ModelState is valid?
 *    - Email not already taken?
 *    ↓
 * 4. Create new User entity
 *    var newUser = new User { ... }
 *    ↓
 * 5. ✅ RAISE DOMAIN EVENT
 *    newUser.AddDomainEvent(new UserRegisteredEvent(newUser));
 *    - This stores the event in newUser._domainEvents list
 *    - Event carries the user data
 *    ↓
 * 6. Add user to DbContext
 *    _dbContext.Users.Add(newUser);
 *    ↓
 * 7. ✅ SAVE CHANGES (CRITICAL!)
 *    await _dbContext.SaveChangesAsync();
 *    ↓
 * 8. Entity Framework commits to database
 *    ↓
 * 9. ✅ INTERCEPTOR ACTIVATES
 *    DomainEventsInterceptor.SavedChangesAsync() runs automatically
 *    ↓
 * 10. Interceptor extracts domain events from the change tracker
 *     - Finds newUser (which has IHasDomainEvents)
 *     - Gets newUser.DomainEvents (contains UserRegisteredEvent)
 *     ↓
 * 11. Interceptor uses reflection to find handlers
 *     - typeof(IDomainEventHandler<UserRegisteredEvent>)
 *     - Finds UserObserver in DI container
 *     ↓
 * 12. ✅ EVENT HANDLER EXECUTES
 *     UserObserver.Handle(userRegisteredEvent) is called
 *     - Sends welcome email to user.Email
 *     - May also log activity, update stats, etc.
 *     ↓
 * 13. Event cleared from entity
 *     newUser.ClearDomainEvents()
 *     ↓
 * 14. Optional: Auto-login user
 *     ↓
 * 15. Redirect to home page
 * 
 * 
 * KEY POINTS:
 * ✅ The event is raised BEFORE SaveChanges
 * ✅ The handler executes AFTER database commit succeeds
 * ✅ If database fails, event is never published (transactional safety)
 * ✅ Multiple handlers can be registered for UserRegisteredEvent
 * ✅ No manual event dispatching needed - interceptor handles it
 * ✅ Decoupled: User doesn't know about email service
 */

// ============================================================================
// WHAT GETS REGISTERED IN Program.cs:
// ============================================================================
/*
builder.Services.AddScoped<IDomainEventHandler<UserRegisteredEvent>, UserObserver>();

This tells the DI container:
- When we need IDomainEventHandler<UserRegisteredEvent>
- Provide an instance of UserObserver
- Scope: Scoped (new instance per request)
*/

// ============================================================================
// VIEW FILE EXAMPLE (Register.cshtml):
// ============================================================================
/*
@model CreateUserViewModel

<div class="form-container">
    <h2>Create Account</h2>
    <form asp-action="DoRegister" asp-controller="Auth" method="post">
        <div class="form-group">
            <label for="Name">Full Name</label>
            <input type="text" id="Name" name="Name" asp-for="Name" required />
            <span asp-validation-for="Name" class="text-danger"></span>
        </div>

        <div class="form-group">
            <label for="Email">Email</label>
            <input type="email" id="Email" name="Email" asp-for="Email" required />
            <span asp-validation-for="Email" class="text-danger"></span>
        </div>

        <div class="form-group">
            <label for="Password">Password</label>
            <input type="password" id="Password" name="Password" asp-for="Password" required />
            <span asp-validation-for="Password" class="text-danger"></span>
        </div>

        <button type="submit" class="btn btn-primary">Register</button>
    </form>

    <p>Already have an account? <a asp-action="Login">Login here</a></p>
</div>
*/
