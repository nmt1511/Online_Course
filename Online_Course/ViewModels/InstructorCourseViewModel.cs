using System.ComponentModel.DataAnnotations;
using Online_Course.Models;

namespace Online_Course.ViewModels;

public class InstructorCourseListViewModel
{
    public int CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int? CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string ThumbnailUrl { get; set; } = string.Empty;
    public CourseStatus Status { get; set; } = CourseStatus.Draft;
    public CourseType Type { get; set; } = CourseType.Fixed_Time;
    public int EnrollmentCount { get; set; }
    public int LessonCount { get; set; }
    public double AverageRating { get; set; }
    
    public string StatusText => Status switch
    {
        CourseStatus.Public => "Công khai",
        CourseStatus.Private => "Riêng tư",
        _ => "Nháp"
    };
    
    public string StatusColor => Status switch
    {
        CourseStatus.Public => "bg-green-500/80",
        CourseStatus.Private => "bg-yellow-500/80",
        _ => "bg-slate-700/80"
    };

    public string TypeText => Type switch
    {
        CourseType.Fixed_Time => "Có thời gian",
        _ => "Xuyên suốt"
    };

    public string TypeColor => Type switch
    {
        CourseType.Fixed_Time => "bg-green-500/80",
        _ => "bg-yellow-500/80"
    };
}

public class InstructorEditCourseViewModel
{
    public int CourseId { get; set; }

    [Required(ErrorMessage = "Tiêu đề khóa học là bắt buộc")]
    [StringLength(200, ErrorMessage = "Tiêu đề không được vượt quá 200 ký tự")]
    [Display(Name = "Tiêu đề khóa học")]
    public string Title { get; set; } = string.Empty;

    [StringLength(2000, ErrorMessage = "Mô tả không được vượt quá 2000 ký tự")]
    [Display(Name = "Mô tả chi tiết")]
    public string Description { get; set; } = string.Empty;

    [Display(Name = "Danh mục")]
    public int? CategoryId { get; set; }

    [StringLength(500, ErrorMessage = "URL ảnh bìa không được vượt quá 500 ký tự")]
    [Display(Name = "URL Ảnh bìa")]
    public string ThumbnailUrl { get; set; } = string.Empty;
    
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

    // Read-only properties for display
    public string InstructorName { get; set; } = string.Empty;
    public int EnrollmentCount { get; set; }
    public int LessonCount { get; set; }
}

public class InstructorDashboardViewModel
{
    public string InstructorName { get; set; } = string.Empty;
    public int TotalStudents { get; set; }
    public int ActiveCourses { get; set; }
    public int DraftCourses { get; set; }
    public double AverageCompletion { get; set; }
    public IEnumerable<InstructorCourseListViewModel> Courses { get; set; } = new List<InstructorCourseListViewModel>();
}

public class InstructorCreateCourseViewModel
{
    [Required(ErrorMessage = "Tiêu đề khóa học là bắt buộc")]
    [StringLength(200, ErrorMessage = "Tiêu đề không được vượt quá 200 ký tự")]
    [Display(Name = "Tiêu đề khóa học")]
    public string Title { get; set; } = string.Empty;

    [StringLength(2000, ErrorMessage = "Mô tả không được vượt quá 2000 ký tự")]
    [Display(Name = "Mô tả chi tiết")]
    public string Description { get; set; } = string.Empty;

    [Display(Name = "Danh mục")]
    public int? CategoryId { get; set; }

    [StringLength(500, ErrorMessage = "URL ảnh bìa không được vượt quá 500 ký tự")]
    [Display(Name = "URL Ảnh bìa")]
    public string ThumbnailUrl { get; set; } = string.Empty;
    
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
}

