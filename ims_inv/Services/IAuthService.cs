using System.Security.Claims;
using ims_inv.Models;

namespace ims_inv.Services
{
    public interface IAuthService
    {
        Task<(bool Success, string? ErrorMessage, ClaimsPrincipal? Principal)> ValidateLoginAsync(string email, string password);
        Task<(bool Success, string? ErrorMessage, User? User)> RegisterUserAsync(CreateUserViewModel model);
        Task<User> CreateAdminUserAsync(string name, string email, string password);
    }
}
