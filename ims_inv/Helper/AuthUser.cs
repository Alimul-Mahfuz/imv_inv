using System.Security.Claims;

namespace ims_inv.Helper
{
    public class AuthUser(IHttpContextAccessor _http)
    {
        public string? UserName = _http.HttpContext?.User?.Identity?.Name;
        public string? Email = _http.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value;
        public string? Id = _http.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }
}
