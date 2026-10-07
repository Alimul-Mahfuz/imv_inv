using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using ims_inv.Events;
using ims_inv.Models;
using ims_inv.Repositories;

namespace ims_inv.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly PasswordHasher<User> _passwordHasher;

        public AuthService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
            _passwordHasher = new PasswordHasher<User>();
        }

        public async Task<(bool Success, string? ErrorMessage, ClaimsPrincipal? Principal)> ValidateLoginAsync(string email, string password)
        {
            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null)
            {
                return (false, "Credentials not found", null);
            }

            var verification = _passwordHasher.VerifyHashedPassword(user, user.Password, password);
            if (verification == PasswordVerificationResult.Failed)
            {
                return (false, "Invalid credentials", null);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Name ?? string.Empty),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            };

            var identity = new ClaimsIdentity(claims, "CookieAuth");
            var principal = new ClaimsPrincipal(identity);

            return (true, null, principal);
        }

        public async Task<(bool Success, string? ErrorMessage, User? User)> RegisterUserAsync(CreateUserViewModel model)
        {
            if (await _userRepository.IsEmailTakenAsync(model.Email))
            {
                return (false, "Email is already registered", null);
            }

            var user = new User
            {
                Name = model.Name ?? string.Empty,
                Email = model.Email
            };

            user.Password = _passwordHasher.HashPassword(user, model.Password);
            user.AddDomainEvent(new UserRegisteredEvent(user));

            await _userRepository.AddAsync(user);
            await _userRepository.SaveChangesAsync();

            return (true, null, user);
        }

        public async Task<User> CreateAdminUserAsync(string name, string email, string password)
        {
            if (await _userRepository.IsEmailTakenAsync(email))
            {
                throw new InvalidOperationException("User with this email already exists");
            }

            var user = new User
            {
                Name = name,
                Email = email
            };

            user.Password = _passwordHasher.HashPassword(user, password);

            await _userRepository.AddAsync(user);
            await _userRepository.SaveChangesAsync();

            return user;
        }
    }
}
