using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ims_inv.Models
{
    /// <summary>
    /// Defines conversion ratios between different units for a specific product
    /// Example: For Coffee, 1 kg = 1000 grams, so conversion factor from gram to kg = 0.001
    /// Each product can have different conversion factors for the same unit pair
    /// </summary>
    public class UnitConversion
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        public int FromUnitId { get; set; }

        [Required]
        public int ToUnitId { get; set; }

        /// <summary>
        /// Conversion factor to convert FROM unit TO target unit for this product
        /// Example: FromUnit=gram, ToUnit=kg, Factor=0.001
        /// So: 1000 grams * 0.001 = 1 kg
        /// </summary>
        [Required]
        public decimal ConversionFactor { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

        // Foreign Keys
        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }

        [ForeignKey("FromUnitId")]
        public virtual Unit FromUnit { get; set; }

        [ForeignKey("ToUnitId")]
        public virtual Unit ToUnit { get; set; }
    }

    public class UnitConversionViewModel
    {
        public int Id { get; set; }

        public int ProductId { get; set; }

        [Required(ErrorMessage = "From Unit is required")]
        [Display(Name = "From Unit")]
        public int FromUnitId { get; set; }

        [Required(ErrorMessage = "To Unit is required")]
        [Display(Name = "To Unit")]
        public int ToUnitId { get; set; }

        [Required(ErrorMessage = "Conversion Factor is required")]
        [Display(Name = "Conversion Factor")]
        [Range(0.0001, 999999.9999, ErrorMessage = "Conversion factor must be between 0.0001 and 999999.9999")]
        public decimal ConversionFactor { get; set; }

        // For display
        public string FromUnitName { get; set; }
        public string FromUnitSymbol { get; set; }
        public string ToUnitName { get; set; }
        public string ToUnitSymbol { get; set; }
    }
}
