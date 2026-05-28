using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ims_inv.Models
{
    /// <summary>
    /// Inventory tracks the quantity of products
    /// </summary>
    public class Inventory
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ProductId { get; set; }

        /// <summary>
        /// Current quantity in stock
        /// </summary>
        [Required]
        public int Quantity { get; set; }

        /// <summary>
        /// Minimum quantity to trigger reorder
        /// </summary>
        public int? ReorderLevel { get; set; }

        /// <summary>
        /// Suggested quantity for reordering
        /// </summary>
        public int? ReorderQuantity { get; set; }

        /// <summary>
        /// Last time inventory was counted/verified
        /// </summary>
        public DateTime? LastCountedAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Foreign Keys
        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }
    }

    public class InventoryViewModel
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public int? ReorderLevel { get; set; }
        public int? ReorderQuantity { get; set; }
        public DateTime? LastCountedAt { get; set; }
    }
}
