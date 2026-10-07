using ims_inv.Models;
using ims_inv.Repositories;
using X.PagedList;
using X.PagedList.Extensions;

namespace ims_inv.Services
{
    public class WarehouseService : IWarehouseService
    {
        private readonly IWarehouseRepository _warehouseRepository;

        public WarehouseService(IWarehouseRepository warehouseRepository)
        {
            _warehouseRepository = warehouseRepository;
        }

        public async Task<List<Warehouse>> GetAllWarehousesAsync()
        {
            return await _warehouseRepository.GetAllAsync();
        }

        public async Task<List<Warehouse>> GetActiveWarehousesAsync()
        {
            return await _warehouseRepository.GetActiveWarehousesAsync();
        }

        public IPagedList<Warehouse> GetPagedWarehouses(int page, int pageSize)
        {
            return _warehouseRepository.Query()
                .OrderByDescending(x => x.Id)
                .ToPagedList(page, pageSize);
        }

        public async Task<Warehouse?> GetWarehouseByIdAsync(int id)
        {
            return await _warehouseRepository.GetByIdAsync(id);
        }

        public async Task<WarehouseViewModel?> GetWarehouseViewModelAsync(int id)
        {
            var warehouse = await _warehouseRepository.GetByIdAsync(id);
            if (warehouse == null)
            {
                return null;
            }

            return new WarehouseViewModel
            {
                Id = warehouse.Id,
                Name = warehouse.Name,
                Address = warehouse.Address,
                Phone = warehouse.Phone,
                Email = warehouse.Email,
                IsActive = warehouse.IsActive,
                Capacity = warehouse.Capacity
            };
        }

        public async Task<(bool Success, string? ErrorMessage, Warehouse? Warehouse)> CreateOrUpdateWarehouseAsync(WarehouseViewModel model)
        {
            if (model.Id == 0)
            {
                var warehouse = new Warehouse
                {
                    Name = model.Name,
                    Address = model.Address,
                    Phone = model.Phone,
                    Email = model.Email,
                    IsActive = model.IsActive,
                    Capacity = model.Capacity
                };

                await _warehouseRepository.AddAsync(warehouse);
                await _warehouseRepository.SaveChangesAsync();

                return (true, null, warehouse);
            }
            else
            {
                var warehouse = await _warehouseRepository.GetByIdAsync(model.Id);
                if (warehouse == null)
                {
                    return (false, "Warehouse not found.", null);
                }

                warehouse.Name = model.Name;
                warehouse.Address = model.Address;
                warehouse.Phone = model.Phone;
                warehouse.Email = model.Email;
                warehouse.IsActive = model.IsActive;
                warehouse.Capacity = model.Capacity;

                _warehouseRepository.Update(warehouse);
                await _warehouseRepository.SaveChangesAsync();

                return (true, null, warehouse);
            }
        }

        public async Task<bool> DeleteWarehouseAsync(int id)
        {
            var warehouse = await _warehouseRepository.GetByIdAsync(id);
            if (warehouse == null)
            {
                return false;
            }

            await _warehouseRepository.DeleteAsync(id);
            await _warehouseRepository.SaveChangesAsync();
            return true;
        }
    }
}
