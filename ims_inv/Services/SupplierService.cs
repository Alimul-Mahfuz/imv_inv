using ims_inv.Models;
using ims_inv.Repositories;

namespace ims_inv.Services
{
    public class SupplierService : ISupplierService
    {
        private readonly ISupplierRepository _supplierRepository;

        public SupplierService(ISupplierRepository supplierRepository)
        {
            _supplierRepository = supplierRepository;
        }

        public async Task<List<Supplier>> GetAllSuppliersAsync()
        {
            return await _supplierRepository.GetAllAsync();
        }

        public async Task<List<Supplier>> GetActiveSuppliersAsync()
        {
            return await _supplierRepository.GetActiveSuppliersAsync();
        }

        public async Task<Supplier?> GetSupplierByIdAsync(int id)
        {
            return await _supplierRepository.GetByIdAsync(id);
        }

        public async Task<SupplierViewModel?> GetSupplierViewModelAsync(int id)
        {
            var supplier = await _supplierRepository.GetByIdAsync(id);
            if (supplier == null)
            {
                return null;
            }

            return new SupplierViewModel
            {
                Id = supplier.Id,
                Name = supplier.Name,
                ContactPerson = supplier.ContactPerson,
                Address = supplier.Address,
                Phone = supplier.Phone,
                Email = supplier.Email,
                IsActive = supplier.IsActive
            };
        }

        public async Task<(bool Success, string? ErrorMessage, Supplier? Supplier)> CreateOrUpdateSupplierAsync(SupplierViewModel model)
        {
            if (model.Id == 0)
            {
                var supplier = new Supplier
                {
                    Name = model.Name,
                    ContactPerson = model.ContactPerson,
                    Address = model.Address,
                    Phone = model.Phone,
                    Email = model.Email,
                    IsActive = model.IsActive
                };

                await _supplierRepository.AddAsync(supplier);
                await _supplierRepository.SaveChangesAsync();

                return (true, null, supplier);
            }
            else
            {
                var supplier = await _supplierRepository.GetByIdAsync(model.Id);
                if (supplier == null)
                {
                    return (false, "Supplier not found.", null);
                }

                supplier.Name = model.Name;
                supplier.ContactPerson = model.ContactPerson;
                supplier.Address = model.Address;
                supplier.Phone = model.Phone;
                supplier.Email = model.Email;
                supplier.IsActive = model.IsActive;

                _supplierRepository.Update(supplier);
                await _supplierRepository.SaveChangesAsync();

                return (true, null, supplier);
            }
        }

        public async Task<bool> DeleteSupplierAsync(int id)
        {
            var supplier = await _supplierRepository.GetByIdAsync(id);
            if (supplier == null)
            {
                return false;
            }

            await _supplierRepository.DeleteAsync(id);
            await _supplierRepository.SaveChangesAsync();
            return true;
        }
    }
}
