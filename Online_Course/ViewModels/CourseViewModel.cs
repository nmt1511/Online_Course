using System.ComponentModel.DataAnnotations;
using Online_Course.Models;

namespace Online_Course.ViewModels;

public class CourseListViewModel
{
    public int CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string ThumbnailUrl { get; set; } = string.Empty;
    public string InstructorName { get; set; } = string.Empty;
    public int InstructorId { get; set; }
    public CourseStatus Status { get; set; } = CourseStatus.Draft;
    public int EnrollmentCount { get; set; }
    
    public string StatusText => Status switch
    {
        CourseStatus.Public => "Công khai",
        CourseStatus.Private => "Riêng tư",
        _ => "Nháp"
    };
    
    public string StatusColor => Status switch
    {
        CourseStatus.Public => "bg-green-500",
        CourseStatus.Private => "bg-yellow-500",
        _ => "bg-slate-500"
    };
}

public class CreateCourseViewModel
{
    [Required(ErrorMessage = "Title is required")]
    [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
    public string Title { get; set; } = string.Empty;

    [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
    public string Description { get; set; } = string.Empty;

    [StringLength(100, ErrorMessage = "Category cannot exceed 100 characters")]
    public string Category { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Thumbnail URL cannot exceed 500 characters")]
    [Display(Name = "Thumbnail URL")]
    public string ThumbnailUrl { get; set; } = string.Empty;

    [Required(ErrorMessage = "Instructor is required")]
    [Display(Name = "Instructor")]
    public int InstructorId { get; set; }

    [Display(Name = "Trạng thái")]
    public CourseStatus Status { get; set; } = CourseStatus.Draft;
}

public class EditCourseViewModel
{
    public int CourseId { get; set; }

    [Required(ErrorMessage = "Title is required")]
    [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
    public string Title { get; set; } = string.Empty;

    [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
    public string Description { get; set; } = string.Empty;


    [StringLength(100, ErrorMessage = "Category cannot exceed 100 characters")]
    public string Category { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Thumbnail URL cannot exceed 500 characters")]
    [Display(Name = "Thumbnail URL")]
    public string ThumbnailUrl { get; set; } = string.Empty;

    [Required(ErrorMessage = "Instructor is required")]
    [Display(Name = "Instructor")]
    public int InstructorId { get; set; }

    [Display(Name = "Trạng thái")]
    public CourseStatus Status { get; set; } = CourseStatus.Draft;
}

public class CourseIndexViewModel
{
    public IEnumerable<CourseListViewModel> Courses { get; set; } = new List<CourseListViewModel>();
    public int TotalCourses { get; set; }
    public int PublishedCourses { get; set; }
    public int DraftCourses { get; set; }
    public string? CategoryFilter { get; set; }
    public string? StatusFilter { get; set; }
    public string? SearchQuery { get; set; }
}
