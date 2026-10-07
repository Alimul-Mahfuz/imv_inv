using ims_inv.Models;

namespace ims_inv.Services
{
    public interface ISupplierService
    {
        Task<List<Supplier>> GetAllSuppliersAsync();
        Task<List<Supplier>> GetActiveSuppliersAsync();
        Task<Supplier?> GetSupplierByIdAsync(int id);
        Task<SupplierViewModel?> GetSupplierViewModelAsync(int id);
        Task<(bool Success, string? ErrorMessage, Supplier? Supplier)> CreateOrUpdateSupplierAsync(SupplierViewModel model);
        Task<bool> DeleteSupplierAsync(int id);
    }
}
