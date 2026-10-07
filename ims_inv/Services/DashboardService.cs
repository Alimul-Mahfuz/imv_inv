using ims_inv.Models;
using ims_inv.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ims_inv.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IProductRepository _productRepository;
        private readonly IInventoryRepository _inventoryRepository;
        private readonly IWarehouseRepository _warehouseRepository;
        private readonly ISupplierRepository _supplierRepository;
        private readonly IUserRepository _userRepository;
        private readonly IStockMovementRepository _stockMovementRepository;

        public DashboardService(
            IProductRepository productRepository,
            IInventoryRepository inventoryRepository,
            IWarehouseRepository warehouseRepository,
            ISupplierRepository supplierRepository,
            IUserRepository userRepository,
            IStockMovementRepository stockMovementRepository)
        {
            _productRepository = productRepository;
            _inventoryRepository = inventoryRepository;
            _warehouseRepository = warehouseRepository;
            _supplierRepository = supplierRepository;
            _userRepository = userRepository;
            _stockMovementRepository = stockMovementRepository;
        }

        public async Task<DashboardViewModel> GetDashboardDataAsync()
        {
            var totalProducts = await _productRepository.CountAsync();
            var totalWarehouses = await _warehouseRepository.CountAsync();
            var totalSuppliers = await _supplierRepository.CountAsync();
            var totalUsers = await _userRepository.CountAsync();

            var totalStockQuantity = await _inventoryRepository.Query().SumAsync(i => (decimal?)i.Quantity) ?? 0m;
            var lowStockItems = await _inventoryRepository.GetLowStockAlertsAsync();
            var recentMovements = await _stockMovementRepository.GetRecentMovementsAsync(8);

            return new DashboardViewModel
            {
                TotalProducts = totalProducts,
                TotalStockQuantity = totalStockQuantity,
                LowStockAlertsCount = lowStockItems.Count,
                TotalWarehouses = totalWarehouses,
                TotalSuppliers = totalSuppliers,
                TotalUsers = totalUsers,
                LowStockItems = lowStockItems,
                RecentStockMovements = recentMovements
            };
        }
    }
}
