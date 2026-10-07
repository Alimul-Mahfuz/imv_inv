using ims_inv.Models;

namespace ims_inv.Services
{
    public interface IDashboardService
    {
        Task<DashboardViewModel> GetDashboardDataAsync();
    }
}
