using System.ComponentModel.DataAnnotations;

namespace ims_inv.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public string SKU { get; set; }

        public int CategoryId { get; set; }
        public int SupplierId { get; set; }
        public int BaseUnitId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public Category Category { get; set; }
        public Supplier Supplier { get; set; }
        public Unit Unit { get; set; }
    }

    public class ProductViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Product name is required")]
        public string Name { get; set; }

        [Required(ErrorMessage = "SKU is required")]
        public string SKU { get; set; }

        [Required(ErrorMessage = "Category is required")]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Supplier is required")]
        [Display(Name = "Supplier")]
        public int SupplierId { get; set; }

        [Required(ErrorMessage = "Base Unit is required")]
        [Display(Name = "Base Unit")]
        public int BaseUnitId { get; set; }
    }
}
