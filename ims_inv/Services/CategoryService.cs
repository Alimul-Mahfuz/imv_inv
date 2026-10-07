using ims_inv.Models;
using ims_inv.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ims_inv.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IProductRepository _productRepository;

        public CategoryService(ICategoryRepository categoryRepository, IProductRepository productRepository)
        {
            _categoryRepository = categoryRepository;
            _productRepository = productRepository;
        }

        public async Task<List<Category>> GetAllCategoriesAsync()
        {
            return await _categoryRepository.GetAllWithParentAsync();
        }

        public async Task<List<Category>> GetEligibleParentCategoriesAsync(int currentCategoryId = 0)
        {
            return await _categoryRepository.Query()
                .Where(c => c.Id != currentCategoryId)
                .ToListAsync();
        }

        public async Task<Category?> GetCategoryByIdAsync(int id)
        {
            return await _categoryRepository.GetByIdAsync(id);
        }

        public async Task<CategoryViewModel?> GetCategoryViewModelAsync(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                return null;
            }

            return new CategoryViewModel
            {
                Id = category.Id,
                Name = category.Name,
                ParentId = category.ParentId
            };
        }

        public async Task<(bool Success, string? ErrorMessage, Category? Category)> CreateOrUpdateCategoryAsync(CategoryViewModel model)
        {
            if (model.Id == 0)
            {
                var category = new Category
                {
                    Name = model.Name,
                    ParentId = model.ParentId,
                    CreatedAt = DateTime.UtcNow
                };

                await _categoryRepository.AddAsync(category);
                await _categoryRepository.SaveChangesAsync();

                return (true, null, category);
            }
            else
            {
                var category = await _categoryRepository.GetByIdAsync(model.Id);
                if (category == null)
                {
                    return (false, "Category not found.", null);
                }

                category.Name = model.Name;
                category.ParentId = model.ParentId;

                _categoryRepository.Update(category);
                await _categoryRepository.SaveChangesAsync();

                return (true, null, category);
            }
        }

        public async Task<(bool Success, string? ErrorMessage)> DeleteCategoryAsync(int id)
        {
            var category = await _categoryRepository.GetByIdWithChildrenAsync(id);
            if (category == null)
            {
                return (false, "Category not found.");
            }

            if (category.Children.Any())
            {
                return (false, "Cannot delete category that has sub-categories.");
            }

            var hasProducts = await _productRepository.Query().AnyAsync(p => p.CategoryId == id);
            if (hasProducts)
            {
                return (false, "Cannot delete category that has assigned products.");
            }

            await _categoryRepository.DeleteAsync(id);
            await _categoryRepository.SaveChangesAsync();

            return (true, null);
        }
    }
}
