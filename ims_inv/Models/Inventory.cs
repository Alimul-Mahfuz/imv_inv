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
}
