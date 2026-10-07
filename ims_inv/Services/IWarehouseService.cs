using ims_inv.Models;
using X.PagedList;

namespace ims_inv.Services
{
    public interface IWarehouseService
    {
        Task<List<Warehouse>> GetAllWarehousesAsync();
        Task<List<Warehouse>> GetActiveWarehousesAsync();
        IPagedList<Warehouse> GetPagedWarehouses(int page, int pageSize);
        Task<Warehouse?> GetWarehouseByIdAsync(int id);
        Task<WarehouseViewModel?> GetWarehouseViewModelAsync(int id);
        Task<(bool Success, string? ErrorMessage, Warehouse? Warehouse)> CreateOrUpdateWarehouseAsync(WarehouseViewModel model);
        Task<bool> DeleteWarehouseAsync(int id);
    }
}
