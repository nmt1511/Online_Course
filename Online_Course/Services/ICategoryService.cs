using Online_Course.Models;

namespace Online_Course.Services;

public interface ICategoryService
{
    Task<IEnumerable<Category>> GetAllCategoriesAsync();
    Task<Category?> GetCategoryByIdAsync(int id);
    Task<Category?> GetCategoryByNameAsync(string name);
    Task<Category> CreateCategoryAsync(Category category);
    Task UpdateCategoryAsync(Category category);
    Task DeleteCategoryAsync(int id);
    Task<int> GetTotalCategoriesCountAsync();
    Task<int> GetActiveCategoriesCountAsync();
    Task<int> GetEmptyCategoriesCountAsync();
    Task<int> GetCourseCountByCategoryAsync(int categoryId);
}
