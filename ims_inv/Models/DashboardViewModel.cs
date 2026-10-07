namespace ims_inv.Models
{
    public class DashboardViewModel
    {
        public int TotalProducts { get; set; }
        public decimal TotalStockQuantity { get; set; }
        public int LowStockAlertsCount { get; set; }
        public int TotalWarehouses { get; set; }
        public int TotalSuppliers { get; set; }
        public int TotalUsers { get; set; }
        public List<StockMovement> RecentStockMovements { get; set; } = new();
        public List<Inventory> LowStockItems { get; set; } = new();
    }
}
