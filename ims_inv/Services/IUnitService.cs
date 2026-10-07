using ims_inv.Models;

namespace ims_inv.Services
{
    public interface IUnitService
    {
        Task<List<Unit>> GetAllUnitsAsync();
        Task<Unit?> GetUnitByIdAsync(int id);
        Task<UnitViewModel?> GetUnitViewModelAsync(int id);
        Task<(bool Success, string? ErrorMessage, Unit? Unit)> CreateOrUpdateUnitAsync(UnitViewModel model);
        Task<bool> DeleteUnitAsync(int id);
    }
}
