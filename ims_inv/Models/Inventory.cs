using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ims_inv.Models
{
    /// <summary>
    /// Inventory tracks the quantity of products per warehouse
    /// </summary>
    public class Inventory
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        public int WarehouseId { get; set; }

        /// <summary>
        /// Current quantity in stock in this warehouse (supports fractional units)
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(18,4)")]
        public decimal Quantity { get; set; }

        /// <summary>
        /// Minimum quantity to trigger reorder for this warehouse
        /// </summary>
        [Column(TypeName = "decimal(18,4)")]
        public decimal? ReorderLevel { get; set; }

        /// <summary>
        /// Suggested quantity for reordering
        /// </summary>
        [Column(TypeName = "decimal(18,4)")]
        public decimal? ReorderQuantity { get; set; }

        /// <summary>
        /// Last time inventory was counted/verified
        /// </summary>
        public DateTime? LastCountedAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ConcurrencyCheck]
        public DateTime? UpdatedAt { get; set; }

        // Foreign Keys
        [ForeignKey("ProductId")]
        public virtual Product? Product { get; set; }

        [ForeignKey("WarehouseId")]
        public virtual Warehouse? Warehouse { get; set; }
    }

    public class InventoryViewModel
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductSKU { get; set; } = string.Empty;
        public string BaseUnitSymbol { get; set; } = string.Empty;
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal? ReorderLevel { get; set; }
        public decimal? ReorderQuantity { get; set; }
        public DateTime? LastCountedAt { get; set; }
    }

    public class StockTransferViewModel
    {
        [Required(ErrorMessage = "Product is required")]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Source warehouse is required")]
        public int FromWarehouseId { get; set; }

        [Required(ErrorMessage = "Destination warehouse is required")]
        public int ToWarehouseId { get; set; }

        [Required(ErrorMessage = "Quantity is required")]
        [Range(0.0001, 999999999.9999, ErrorMessage = "Quantity must be greater than zero")]
        public decimal Quantity { get; set; }

        public int? EntryUnitId { get; set; }

        [StringLength(50)]
        public string? ReferenceNumber { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }
    }

    public class StockAdjustmentViewModel
    {
        [Required(ErrorMessage = "Product is required")]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Warehouse is required")]
        public int WarehouseId { get; set; }

        public decimal CurrentSystemQuantity { get; set; }

        [Required(ErrorMessage = "Physical count is required")]
        [Range(0, 999999999.9999, ErrorMessage = "Physical count cannot be negative")]
        public decimal PhysicalCount { get; set; }

        [Required(ErrorMessage = "Reason is required")]
        [StringLength(100)]
        public string Reason { get; set; } = "Physical Count Verification";

        [StringLength(500)]
        public string? Notes { get; set; }
    }

    public class EditReorderSettingsViewModel
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductSKU { get; set; } = string.Empty;
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; } = string.Empty;
        public string UnitSymbol { get; set; } = string.Empty;
        public decimal CurrentQuantity { get; set; }

        [Range(0, 999999999.9999, ErrorMessage = "Reorder level must be non-negative")]
        public decimal? ReorderLevel { get; set; }

        [Range(0, 999999999.9999, ErrorMessage = "Reorder quantity must be non-negative")]
        public decimal? ReorderQuantity { get; set; }
    }

    public class StockCardMovementItem
    {
        public DateTime Date { get; set; }
        public string MovementType { get; set; } = string.Empty;
        public string WarehouseName { get; set; } = string.Empty;
        public decimal QuantityIn { get; set; }
        public decimal QuantityOut { get; set; }
        public decimal RunningBalance { get; set; }
        public string? ReferenceNumber { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public string? RecordedBy { get; set; }
    }

    public class StockCardViewModel
    {
        public Product Product { get; set; } = default!;
        public int? SelectedWarehouseId { get; set; }
        public List<Warehouse> Warehouses { get; set; } = new();
        public decimal TotalQuantity { get; set; }
        public List<StockCardMovementItem> Movements { get; set; } = new();
    }
}
