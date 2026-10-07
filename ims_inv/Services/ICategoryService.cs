using ims_inv.Models;

namespace ims_inv.Services
{
    public interface ICategoryService
    {
        Task<List<Category>> GetAllCategoriesAsync();
        Task<List<Category>> GetEligibleParentCategoriesAsync(int currentCategoryId = 0);
        Task<Category?> GetCategoryByIdAsync(int id);
        Task<CategoryViewModel?> GetCategoryViewModelAsync(int id);
        Task<(bool Success, string? ErrorMessage, Category? Category)> CreateOrUpdateCategoryAsync(CategoryViewModel model);
        Task<(bool Success, string? ErrorMessage)> DeleteCategoryAsync(int id);
    }
}
