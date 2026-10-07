namespace ims_inv.Models
{
    public class UnitManagementViewModel
    {
        public List<Unit> Units { get; set; } = new();
        public List<UnitConversion> Conversions { get; set; } = new();
        public List<Product> Products { get; set; } = new();
        public CreateUnitConversionViewModel NewConversion { get; set; } = new();
    }
}
