using System.ComponentModel.DataAnnotations;
using Online_Course.Models;

namespace Online_Course.ViewModels;

public class CourseListViewModel
{
    public int CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int? CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
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
    [Required(ErrorMessage = "Tiêu đề khóa học là thông tin bắt buộc")]
    [StringLength(200, ErrorMessage = "Tiêu đề không được phép vượt quá 200 ký tự")]
    public string Title { get; set; } = string.Empty;

    [StringLength(2000, ErrorMessage = "Mô tả khóa học không được vượt quá 2000 ký tự")]
    public string Description { get; set; } = string.Empty;

    [Display(Name = "Danh mục")]
    public int? CategoryId { get; set; }

    [StringLength(500, ErrorMessage = "Đường dẫn Thumbnail không được vượt quá 500 ký tự")]
    [Display(Name = "Thumbnail URL")]
    public string ThumbnailUrl { get; set; } = string.Empty;

    [Required(ErrorMessage = "Thông tin Giảng viên là bắt buộc")]
    [Display(Name = "Instructor")]
    public int InstructorId { get; set; }

    [Display(Name = "Trạng thái")]
    public CourseStatus Status { get; set; } = CourseStatus.Draft;

    [Display(Name = "Loại khóa học")]
    public CourseType CourseType { get; set; } = CourseType.Open_Always;

    [Display(Name = "Ngày bắt đầu đăng ký")]
    [DataType(DataType.Date)]
    public DateTime? RegistrationStartDate { get; set; }

    [Display(Name = "Ngày kết thúc đăng ký")]
    [DataType(DataType.Date)]
    public DateTime? RegistrationEndDate { get; set; }

    [Display(Name = "Ngày bắt đầu học")]
    [DataType(DataType.Date)]
    public DateTime? StartDate { get; set; }

    [Display(Name = "Ngày kết thúc học")]
    [DataType(DataType.Date)]
    public DateTime? EndDate { get; set; }

    [Display(Name = "Chọn học viên (cho khóa học riêng tư)")]
    public List<int>? SelectedStudentIds { get; set; }
}

public class EditCourseViewModel
{
    public int CourseId { get; set; }

    [Required(ErrorMessage = "Tiêu đề khóa học là thông tin bắt buộc")]
    [StringLength(200, ErrorMessage = "Tiêu đề không được vượt quá 200 ký tự")]
    public string Title { get; set; } = string.Empty;

    [StringLength(2000, ErrorMessage = "Mô tả khóa học không được vượt quá 2000 ký tự")]
    public string Description { get; set; } = string.Empty;

    [Display(Name = "Danh mục")]
    public int? CategoryId { get; set; }

    [StringLength(500, ErrorMessage = "Đường dẫn Thumbnail không được vượt quá 500 ký tự")]
    [Display(Name = "Thumbnail URL")]
    public string ThumbnailUrl { get; set; } = string.Empty;

    [Required(ErrorMessage = "Thông tin Giảng viên là bắt buộc")]
    [Display(Name = "Instructor")]
    public int InstructorId { get; set; }

    [Display(Name = "Trạng thái")]
    public CourseStatus Status { get; set; } = CourseStatus.Draft;

    [Display(Name = "Loại khóa học")]
    public CourseType CourseType { get; set; } = CourseType.Open_Always;

    [Display(Name = "Ngày bắt đầu đăng ký")]
    [DataType(DataType.Date)]
    public DateTime? RegistrationStartDate { get; set; }

    [Display(Name = "Ngày kết thúc đăng ký")]
    [DataType(DataType.Date)]
    public DateTime? RegistrationEndDate { get; set; }

    [Display(Name = "Ngày bắt đầu học")]
    [DataType(DataType.Date)]
    public DateTime? StartDate { get; set; }

    [Display(Name = "Ngày kết thúc học")]
    [DataType(DataType.Date)]
    public DateTime? EndDate { get; set; }

    [Display(Name = "Chọn học viên (cho khóa học riêng tư)")]
    public List<int>? SelectedStudentIds { get; set; }
}

public class CourseIndexViewModel
{
    // Cấu trúc dữ liệu phục vụ trang Theo dõi tiến độ học tập của học viên (Student Progress Tracking)
    public IEnumerable<CourseListViewModel> Courses { get; set; } = new List<CourseListViewModel>();
    public int TotalCourses { get; set; }
    public int PublishedCourses { get; set; }
    public int DraftCourses { get; set; }
    public int? CategoryFilter { get; set; }
    public string? StatusFilter { get; set; }

    // Các thuộc tính hỗ trợ phân trang dữ liệu (Pagination)
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public bool HasPreviousPage => CurrentPage > 1;
    public bool HasNextPage => CurrentPage < TotalPages;

    public string? SearchQuery { get; internal set; }
}
