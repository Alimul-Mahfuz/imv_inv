using ims_inv.Models;
using ims_inv.Repositories;

namespace ims_inv.Services
{
    public class UnitService : IUnitService
    {
        private readonly IUnitRepository _unitRepository;

        public UnitService(IUnitRepository unitRepository)
        {
            _unitRepository = unitRepository;
        }

        public async Task<List<Unit>> GetAllUnitsAsync()
        {
            return await _unitRepository.GetAllAsync();
        }

        public async Task<Unit?> GetUnitByIdAsync(int id)
        {
            return await _unitRepository.GetByIdAsync(id);
        }

        public async Task<UnitViewModel?> GetUnitViewModelAsync(int id)
        {
            var unit = await _unitRepository.GetByIdAsync(id);
            if (unit == null)
            {
                return null;
            }

            return new UnitViewModel
            {
                Id = unit.Id,
                Name = unit.Name,
                Symbol = unit.Symbol,
                Type = unit.Type
            };
        }

        public async Task<(bool Success, string? ErrorMessage, Unit? Unit)> CreateOrUpdateUnitAsync(UnitViewModel model)
        {
            if (model.Id == 0)
            {
                var unit = new Unit
                {
                    Name = model.Name,
                    Symbol = model.Symbol,
                    Type = model.Type,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitRepository.AddAsync(unit);
                await _unitRepository.SaveChangesAsync();

                return (true, null, unit);
            }
            else
            {
                var unit = await _unitRepository.GetByIdAsync(model.Id);
                if (unit == null)
                {
                    return (false, "Unit not found.", null);
                }

                unit.Name = model.Name;
                unit.Symbol = model.Symbol;
                unit.Type = model.Type;

                _unitRepository.Update(unit);
                await _unitRepository.SaveChangesAsync();

                return (true, null, unit);
            }
        }

        public async Task<bool> DeleteUnitAsync(int id)
        {
            var unit = await _unitRepository.GetByIdAsync(id);
            if (unit == null)
            {
                return false;
            }

            await _unitRepository.DeleteAsync(id);
            await _unitRepository.SaveChangesAsync();
            return true;
        }
    }
}
