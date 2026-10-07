using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ims_inv.Models
{
    /// <summary>
    /// StockMovement tracks all incoming and outgoing stock transactions
    /// </summary>
    public class StockMovement
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        public int WarehouseId { get; set; }

        /// <summary>
        /// IN = stock received, OUT = stock issued
        /// </summary>
        [Required]
        [StringLength(10)]
        public string MovementType { get; set; } = string.Empty; // "IN" or "OUT"

        /// <summary>
        /// Quantity moved in base unit
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(18,4)")]
        public decimal Quantity { get; set; }

        /// <summary>
        /// Reference number (PO number, SO number, etc.)
        /// </summary>
        [StringLength(50)]
        public string? ReferenceNumber { get; set; }

        /// <summary>
        /// Reason for movement (Purchase, Sales, Damage, Adjustment, etc.)
        /// </summary>
        [Required]
        [StringLength(100)]
        public string Reason { get; set; } = string.Empty;

        /// <summary>
        /// Additional notes
        /// </summary>
        [StringLength(500)]
        public string? Notes { get; set; }

        /// <summary>
        /// User who recorded the movement
        /// </summary>
        public int? UserId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Foreign Keys
        [ForeignKey("ProductId")]
        public virtual Product? Product { get; set; }

        [ForeignKey("WarehouseId")]
        public virtual Warehouse? Warehouse { get; set; }

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }
    }

    public class StockMovementViewModel
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; } = string.Empty;
        public string MovementType { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public string? ReferenceNumber { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public int? UserId { get; set; }
        public string? UserName { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// For creating stock movements
    /// </summary>
    public class CreateStockMovementViewModel
    {
        [Required(ErrorMessage = "Product is required")]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Warehouse is required")]
        public int WarehouseId { get; set; }

        [Required(ErrorMessage = "Movement type is required")]
        [RegularExpression("^(IN|OUT)$", ErrorMessage = "Movement type must be 'IN' or 'OUT'")]
        public string MovementType { get; set; } = "IN"; // "IN" or "OUT"

        /// <summary>
        /// Quantity entered by user (in selected unit)
        /// </summary>
        [Required(ErrorMessage = "Quantity is required")]
        [Range(0.0001, 999999999.9999, ErrorMessage = "Quantity must be greater than 0")]
        public decimal Quantity { get; set; }

        /// <summary>
        /// Unit ID for entry (defaults to product's base unit)
        /// </summary>
        public int? EntryUnitId { get; set; }

        [StringLength(50)]
        public string? ReferenceNumber { get; set; }

        [Required(ErrorMessage = "Reason is required")]
        [StringLength(100)]
        public string Reason { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Notes { get; set; }

        // For display purposes
        public string? ProductName { get; set; }
        public string? ProductBaseUnitName { get; set; }
        public int ProductBaseUnitId { get; set; }
        public decimal ConvertedQuantity { get; set; } // Quantity in base unit
    }
}
