using ims_inv.Models;

namespace ims_inv.Services
{
    public interface IProductService
    {
        Task<List<Product>> GetAllProductsAsync();
        Task<Product?> GetProductByIdAsync(int id);
        Task<ProductViewModel?> GetProductViewModelAsync(int id);
        Task<(bool Success, string? ErrorMessage, Product? Product)> CreateOrUpdateProductAsync(ProductViewModel model);
        Task<bool> IsSkuUniqueAsync(string sku, int? excludeId = null);
        Task<bool> DeleteProductAsync(int id);
    }
}
