using System.ComponentModel.DataAnnotations;

namespace Online_Course.ViewModels;

public class CategoryIndexViewModel
{
    public IEnumerable<CategoryListViewModel> Categories { get; set; } = new List<CategoryListViewModel>();
    public int TotalCategories { get; set; }
    public int ActiveCategories { get; set; }
    public int EmptyCategories { get; set; }
    public string? SearchQuery { get; set; }
}

public class CategoryListViewModel
{
    public int CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Slug { get; set; }
    public int CourseCount { get; set; }
    public bool IsActive { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateCategoryViewModel
{
    [Required(ErrorMessage = "Category name is required")]
    [StringLength(100, ErrorMessage = "Category name cannot exceed 100 characters")]
    public string Name { get; set; } = string.Empty;
}

public class EditCategoryViewModel
{
    public int CategoryId { get; set; }
    
    [Required(ErrorMessage = "Category name is required")]
    [StringLength(100, ErrorMessage = "Category name cannot exceed 100 characters")]
    public string Name { get; set; } = string.Empty;
    
    public bool IsActive { get; set; } = true;
}
