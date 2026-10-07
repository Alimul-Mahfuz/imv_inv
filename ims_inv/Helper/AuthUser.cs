using System.Security.Claims;

namespace ims_inv.Helper
{
    public class AuthUser
    {
        private readonly IHttpContextAccessor _http;

        public AuthUser(IHttpContextAccessor http)
        {
            _http = http;
        }

        public string? UserName => _http.HttpContext?.User?.Identity?.Name;
        public string? Email => _http.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value;
        public string? IdString => _http.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        public int? Id => int.TryParse(IdString, out var parsedId) ? parsedId : null;
        public bool IsAuthenticated => _http.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
    }
}

