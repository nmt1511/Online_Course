using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Online_Course.Models;
using Online_Course.Services.CategoryService;
using Online_Course.ViewModels;

namespace Online_Course.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "AdminOnly")]
public class CategoriesController : Controller
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    // Displays a list of course categories with search filters and quick statistics.
    public async Task<IActionResult> Index(string? search)
    {
        var categories = await _categoryService.GetAllCategoriesAsync();
        var totalCategories = await _categoryService.GetTotalCategoriesCountAsync();
        var activeCategories = await _categoryService.GetActiveCategoriesCountAsync();
        var emptyCategories = await _categoryService.GetEmptyCategoriesCountAsync();

        // Thực hiện lọc danh sách danh mục nếu có từ khóa tìm kiếm
        if (!string.IsNullOrEmpty(search))
        {
            categories = categories.Where(c => 
                c.Name.Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        var categoryList = new List<CategoryListViewModel>();
        foreach (var category in categories)
        {
            var courseCount = await _categoryService.GetCourseCountByCategoryAsync(category.CategoryId);
            categoryList.Add(new CategoryListViewModel
            {
                CategoryId = category.CategoryId,
                Name = category.Name,
                Slug = category.Slug,
                CourseCount = courseCount,
                IsActive = category.IsActive,
                UpdatedAt = category.UpdatedAt,
                CreatedAt = category.CreatedAt
            });
        }


        var viewModel = new CategoryIndexViewModel
        {
            Categories = categoryList,
            TotalCategories = totalCategories,
            ActiveCategories = activeCategories,
            EmptyCategories = emptyCategories,
            SearchQuery = search
        };

        return View(viewModel);
    }

    // Khởi tạo một danh mục khóa học mới vào hệ thống
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateCategoryViewModel model)
    {
        if (ModelState.IsValid)
        {
            // Kiểm tra tính duy nhất của tên danh mục trước khi lưu
            var existingCategory = await _categoryService.GetCategoryByNameAsync(model.Name);
            if (existingCategory != null)
            {
                TempData["ErrorMessage"] = "Danh mục với tên này đã tồn tại.";
                return RedirectToAction(nameof(Index));
            }

            var category = new Category
            {
                Name = model.Name,
                IsActive = model.IsActive
            };

            await _categoryService.CreateCategoryAsync(category);
            TempData["SuccessMessage"] = "Thêm danh mục thành công!";
            return RedirectToAction(nameof(Index));
        }

        TempData["ErrorMessage"] = "Dữ liệu danh mục không hợp lệ.";
        return RedirectToAction(nameof(Index));
    }

    // Hiển thị form chỉnh sửa thông tin danh mục theo ID
    public async Task<IActionResult> Edit(int id)
    {
        var category = await _categoryService.GetCategoryByIdAsync(id);
        if (category == null)
        {
            TempData["ErrorMessage"] = "Không tìm thấy danh mục.";
            return RedirectToAction(nameof(Index));
        }

        var viewModel = new EditCategoryViewModel
        {
            CategoryId = category.CategoryId,
            Name = category.Name,
            IsActive = category.IsActive
        };

        return View(viewModel);
    }

    // Cập nhật thông tin thay đổi của danh mục vào cơ sở dữ liệu
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, EditCategoryViewModel model)
    {
        if (id != model.CategoryId)
        {
            TempData["ErrorMessage"] = "ID danh mục không hợp lệ.";
            return RedirectToAction(nameof(Index));
        }

        if (ModelState.IsValid)
        {
            var existingCategory = await _categoryService.GetCategoryByIdAsync(id);
            if (existingCategory == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy danh mục.";
                return RedirectToAction(nameof(Index));
            }

            // Đảm bảo tên danh mục mới không trùng lặp với các danh mục khác đã có sẵn
            var categoryWithSameName = await _categoryService.GetCategoryByNameAsync(model.Name);
            if (categoryWithSameName != null && categoryWithSameName.CategoryId != id)
            {
                TempData["ErrorMessage"] = "Danh mục với tên này đã tồn tại.";
                return View(model);
            }

            existingCategory.Name = model.Name;
            existingCategory.IsActive = model.IsActive;

            await _categoryService.UpdateCategoryAsync(existingCategory);
            TempData["SuccessMessage"] = "Cập nhật danh mục thành công!";
            return RedirectToAction(nameof(Index));
        }

        return View(model);
    }

    // Loại bỏ danh mục khỏi hệ thống sau khi kiểm tra các ràng buộc liên quan
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var category = await _categoryService.GetCategoryByIdAsync(id);
        if (category == null)
        {
            TempData["ErrorMessage"] = "Không tìm thấy danh mục.";
            return RedirectToAction(nameof(Index));
        }

        // Kiểm tra xem danh mục hiện có đang chứa khóa học nào không (ràng buộc toàn vẹn dữ liệu)
        var courseCount = await _categoryService.GetCourseCountByCategoryAsync(category.CategoryId);
        if (courseCount > 0)
        {
            TempData["ErrorMessage"] = $"Không thể xóa danh mục '{category.Name}' vì có {courseCount} khóa học đang sử dụng.";
            return RedirectToAction(nameof(Index));
        }

        await _categoryService.DeleteCategoryAsync(id);
        TempData["SuccessMessage"] = "Xóa danh mục thành công!";
        return RedirectToAction(nameof(Index));
    }
}
