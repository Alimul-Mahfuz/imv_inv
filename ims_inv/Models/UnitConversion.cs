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
        [Column(TypeName = "decimal(18,4)")]
        public decimal ConversionFactor { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Foreign Keys
        [ForeignKey("ProductId")]
        public virtual Product? Product { get; set; }

        [ForeignKey("FromUnitId")]
        public virtual Unit? FromUnit { get; set; }

        [ForeignKey("ToUnitId")]
        public virtual Unit? ToUnit { get; set; }
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
        public string FromUnitName { get; set; } = string.Empty;
        public string FromUnitSymbol { get; set; } = string.Empty;
        public string ToUnitName { get; set; } = string.Empty;
        public string ToUnitSymbol { get; set; } = string.Empty;
    }

    public class ProductUnitConversionViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductSKU { get; set; } = string.Empty;
        public int BaseUnitId { get; set; }
        public string BaseUnitName { get; set; } = string.Empty;
        public string BaseUnitSymbol { get; set; } = string.Empty;
        public List<UnitConversionViewModel> Conversions { get; set; } = new();
        public List<Unit> AllUnits { get; set; } = new();
    }

    public class CreateUnitConversionViewModel
    {
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public int BaseUnitId { get; set; }
        public string? BaseUnitName { get; set; }

        [Required(ErrorMessage = "Target unit is required")]
        [Display(Name = "Convert To")]
        public int TargetUnitId { get; set; }

        [Required(ErrorMessage = "Conversion factor is required")]
        [Range(0.0001, 999999.9999, ErrorMessage = "Conversion factor must be between 0.0001 and 999999.9999")]
        [Display(Name = "Conversion Factor")]
        public decimal ConversionFactor { get; set; }

        public List<Unit> AvailableUnits { get; set; } = new();
    }

    public class EditUnitConversionViewModel
    {
        public int ConversionId { get; set; }
        public int ProductId { get; set; }
        public string FromUnitName { get; set; } = string.Empty;
        public string FromUnitSymbol { get; set; } = string.Empty;
        public string ToUnitName { get; set; } = string.Empty;
        public string ToUnitSymbol { get; set; } = string.Empty;

        [Required(ErrorMessage = "Conversion factor is required")]
        [Range(0.0001, 999999.9999, ErrorMessage = "Conversion factor must be between 0.0001 and 999999.9999")]
        [Display(Name = "Conversion Factor")]
        public decimal ConversionFactor { get; set; }
    }
}
