using Microsoft.EntityFrameworkCore;
using Online_Course.Data;
using Online_Course.Models;
using System.Text.RegularExpressions;

namespace Online_Course.Services;

public class CategoryService : ICategoryService
{
    private readonly ApplicationDbContext _context;

    public CategoryService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
    {
        return await _context.Categories
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<Category?> GetCategoryByIdAsync(int id)
    {
        return await _context.Categories.FindAsync(id);
    }

    public async Task<Category?> GetCategoryByNameAsync(string name)
    {
        return await _context.Categories
            .FirstOrDefaultAsync(c => c.Name.ToLower() == name.ToLower());
    }

    public async Task<Category> CreateCategoryAsync(Category category)
    {
        category.Slug = GenerateSlug(category.Name);
        category.CreatedAt = DateTime.UtcNow;
        
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();
        return category;
    }

    public async Task UpdateCategoryAsync(Category category)
    {
        var existingCategory = await _context.Categories.FindAsync(category.CategoryId);
        if (existingCategory != null)
        {
            existingCategory.Name = category.Name;
            existingCategory.Slug = GenerateSlug(category.Name);
            existingCategory.UpdatedAt = DateTime.UtcNow;
            existingCategory.IsActive = category.IsActive;
            
            await _context.SaveChangesAsync();
        }
    }

    public async Task DeleteCategoryAsync(int id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category != null)
        {
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
        }
    }


    public async Task<int> GetTotalCategoriesCountAsync()
    {
        return await _context.Categories.CountAsync();
    }

    public async Task<int> GetActiveCategoriesCountAsync()
    {
        var categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();
        var activeCount = 0;
        
        foreach (var category in categories)
        {
            var courseCount = await GetCourseCountByCategoryAsync(category.Name);
            if (courseCount > 0)
                activeCount++;
        }
        
        return activeCount;
    }

    public async Task<int> GetEmptyCategoriesCountAsync()
    {
        var categories = await _context.Categories.ToListAsync();
        var emptyCount = 0;
        
        foreach (var category in categories)
        {
            var courseCount = await GetCourseCountByCategoryAsync(category.Name);
            if (courseCount == 0)
                emptyCount++;
        }
        
        return emptyCount;
    }

    public async Task<int> GetCourseCountByCategoryAsync(string categoryName)
    {
        return await _context.Courses
            .CountAsync(c => c.Category == categoryName);
    }

    private static string GenerateSlug(string name)
    {
        var slug = name.ToLower();
        slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");
        slug = Regex.Replace(slug, @"\s+", "-");
        slug = Regex.Replace(slug, @"-+", "-");
        slug = slug.Trim('-');
        return slug;
    }
}
