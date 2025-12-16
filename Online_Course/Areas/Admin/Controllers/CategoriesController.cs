using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Online_Course.Models;
using Online_Course.Services;
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

    // GET: Admin/Categories
    public async Task<IActionResult> Index(string? search)
    {
        var categories = await _categoryService.GetAllCategoriesAsync();
        var totalCategories = await _categoryService.GetTotalCategoriesCountAsync();
        var activeCategories = await _categoryService.GetActiveCategoriesCountAsync();
        var emptyCategories = await _categoryService.GetEmptyCategoriesCountAsync();

        // Apply search filter
        if (!string.IsNullOrEmpty(search))
        {
            categories = categories.Where(c => 
                c.Name.Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        var categoryList = new List<CategoryListViewModel>();
        foreach (var category in categories)
        {
            var courseCount = await _categoryService.GetCourseCountByCategoryAsync(category.Name);
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

    // POST: Admin/Categories/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateCategoryViewModel model)
    {
        if (ModelState.IsValid)
        {
            // Check if category already exists
            var existingCategory = await _categoryService.GetCategoryByNameAsync(model.Name);
            if (existingCategory != null)
            {
                TempData["ErrorMessage"] = "A category with this name already exists.";
                return RedirectToAction(nameof(Index));
            }

            var category = new Category
            {
                Name = model.Name,
                IsActive = true
            };

            await _categoryService.CreateCategoryAsync(category);
            TempData["SuccessMessage"] = "Category created successfully!";
            return RedirectToAction(nameof(Index));
        }

        TempData["ErrorMessage"] = "Invalid category data.";
        return RedirectToAction(nameof(Index));
    }

    // GET: Admin/Categories/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var category = await _categoryService.GetCategoryByIdAsync(id);
        if (category == null)
        {
            TempData["ErrorMessage"] = "Category not found.";
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

    // POST: Admin/Categories/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, EditCategoryViewModel model)
    {
        if (id != model.CategoryId)
        {
            TempData["ErrorMessage"] = "Invalid category ID.";
            return RedirectToAction(nameof(Index));
        }

        if (ModelState.IsValid)
        {
            var existingCategory = await _categoryService.GetCategoryByIdAsync(id);
            if (existingCategory == null)
            {
                TempData["ErrorMessage"] = "Category not found.";
                return RedirectToAction(nameof(Index));
            }

            // Check if another category with the same name exists
            var categoryWithSameName = await _categoryService.GetCategoryByNameAsync(model.Name);
            if (categoryWithSameName != null && categoryWithSameName.CategoryId != id)
            {
                TempData["ErrorMessage"] = "A category with this name already exists.";
                return View(model);
            }

            existingCategory.Name = model.Name;
            existingCategory.IsActive = model.IsActive;

            await _categoryService.UpdateCategoryAsync(existingCategory);
            TempData["SuccessMessage"] = "Category updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        return View(model);
    }

    // POST: Admin/Categories/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var category = await _categoryService.GetCategoryByIdAsync(id);
        if (category == null)
        {
            TempData["ErrorMessage"] = "Category not found.";
            return RedirectToAction(nameof(Index));
        }

        // Check if category has courses
        var courseCount = await _categoryService.GetCourseCountByCategoryAsync(category.Name);
        if (courseCount > 0)
        {
            TempData["ErrorMessage"] = $"Cannot delete category '{category.Name}' because it has {courseCount} course(s) assigned.";
            return RedirectToAction(nameof(Index));
        }

        await _categoryService.DeleteCategoryAsync(id);
        TempData["SuccessMessage"] = "Category deleted successfully!";
        return RedirectToAction(nameof(Index));
    }
}
