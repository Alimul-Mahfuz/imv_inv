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
        public string MovementType { get; set; }  // "IN" or "OUT"

        /// <summary>
        /// Quantity moved
        /// </summary>
        [Required]
        public int Quantity { get; set; }

        /// <summary>
        /// Reference number (PO number, SO number, etc.)
        /// </summary>
        public string ReferenceNumber { get; set; }

        /// <summary>
        /// Reason for movement (Purchase, Sales, Damage, Adjustment, etc.)
        /// </summary>
        [Required]
        public string Reason { get; set; }

        /// <summary>
        /// Additional notes
        /// </summary>
        public string Notes { get; set; }

        /// <summary>
        /// User who recorded the movement
        /// </summary>
        public int? UserId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Foreign Keys
        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }

        [ForeignKey("WarehouseId")]
        public virtual Warehouse Warehouse { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }
    }

    public class StockMovementViewModel
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public string MovementType { get; set; }
        public int Quantity { get; set; }
        public string ReferenceNumber { get; set; }
        public string Reason { get; set; }
        public string? Notes { get; set; }
        public int? UserId { get; set; }
        public string UserName { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// For creating/editing stock movements
    /// </summary>
    public class CreateStockMovementViewModel
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        public int WarehouseId { get; set; }

        [Required]
        public string MovementType { get; set; }  // "IN" or "OUT"

        /// <summary>
        /// Quantity entered by user (in selected unit)
        /// </summary>
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public decimal Quantity { get; set; }

        /// <summary>
        /// Unit ID for entry (defaults to product's base unit)
        /// </summary>
        public int? EntryUnitId { get; set; }

        public string ReferenceNumber { get; set; }

        [Required]
        public string Reason { get; set; }

        public string? Notes { get; set; } = string.Empty;

        // For display purposes
        public string? ProductName { get; set; }
        public string? ProductBaseUnitName { get; set; }
        public int ProductBaseUnitId { get; set; }
        public decimal ConvertedQuantity { get; set; }  // Quantity in base unit
    }
}
