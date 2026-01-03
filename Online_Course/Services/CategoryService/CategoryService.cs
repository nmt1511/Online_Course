using Microsoft.EntityFrameworkCore;
using Online_Course.Data;
using Online_Course.Models;
using System.Text.RegularExpressions;

namespace Online_Course.Services.CategoryService;

public class CategoryService : ICategoryService
{
    private readonly ApplicationDbContext _context;

    public CategoryService(ApplicationDbContext context)
    {
        _context = context;
    }

    // Lấy danh mục khóa học, sắp xếp theo thời gian tạo giảm dần
    public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
    {
        return await _context.Categories
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    // Tìm kiếm danh mục theo ID
    public async Task<Category?> GetCategoryByIdAsync(int id)
    {
        return await _context.Categories.FindAsync(id);
    }

    // Tìm kiếm danh mục theo tên (không phân biệt hoa thường)
    public async Task<Category?> GetCategoryByNameAsync(string name)
    {
        return await _context.Categories
            .FirstOrDefaultAsync(c => c.Name.ToLower() == name.ToLower());
    }

    // Khởi tạo danh mục mới, tự động sinh Slug và gán thời gian tạo
    public async Task<Category> CreateCategoryAsync(Category category)
    {
        category.Slug = GenerateSlug(category.Name);
        category.CreatedAt = DateTime.UtcNow;
        
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();
        return category;
    }

    // Cập nhật thông tin danh mục và cập nhật lại Slug
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

    // Xóa danh mục khỏi hệ thống
    public async Task DeleteCategoryAsync(int id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category != null)
        {
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
        }
    }


    // Thống kê tổng số lượng danh mục
    public async Task<int> GetTotalCategoriesCountAsync()
    {
        return await _context.Categories.CountAsync();
    }

    // Thống kê số lượng danh mục đang có khóa học hoạt động
    public async Task<int> GetActiveCategoriesCountAsync()
    {
        var categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();
        var activeCount = 0;
        
        foreach (var category in categories)
        {
            var courseCount = await GetCourseCountByCategoryAsync(category.CategoryId);
            if (courseCount > 0)
                activeCount++;
        }
        
        return activeCount;
    }

    // Thống kê số lượng danh mục chưa có khóa học nào
    public async Task<int> GetEmptyCategoriesCountAsync()
    {
        var categories = await _context.Categories.ToListAsync();
        var emptyCount = 0;
        
        foreach (var category in categories)
        {
            var courseCount = await GetCourseCountByCategoryAsync(category.CategoryId);
            if (courseCount == 0)
                emptyCount++;
        }
        
        return emptyCount;
    }

    // Đếm số lượng khóa học thuộc một danh mục cụ thể
    public async Task<int> GetCourseCountByCategoryAsync(int categoryId)
    {
        return await _context.Courses
            .CountAsync(c => c.CategoryId == categoryId);
    }

    // Chuyển đổi tên danh mục thành dạng Slug (không dấu, không ký tự đặc biệt)
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
