using System.ComponentModel.DataAnnotations;

namespace Online_Course.ViewModels;

public class UserListViewModel
{
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateUserViewModel
{
    [Required(ErrorMessage = "Họ và tên là bắt buộc")]
    [StringLength(100, ErrorMessage = "Họ và tên không được vượt quá 100 ký tự")]
    [Display(Name = "Họ và tên")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email là bắt buộc")]
    [EmailAddress(ErrorMessage = "Định dạng Email không hợp lệ")]
    [StringLength(256, ErrorMessage = "Email không được vượt quá 256 ký tự")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vai trò người dùng là bắt buộc")]
    [Display(Name = "Vai trò")]
    public int RoleId { get; set; }
}

public class EditUserViewModel
{
    public int UserId { get; set; }

    [Required(ErrorMessage = "Họ và tên là bắt buộc")]
    [StringLength(100, ErrorMessage = "Họ và tên không được vượt quá 100 ký tự")]
    [Display(Name = "Họ và tên")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email là bắt buộc")]
    [EmailAddress(ErrorMessage = "Định dạng Email không hợp lệ")]
    [StringLength(256, ErrorMessage = "Email không được vượt quá 256 ký tự")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vai trò người dùng là bắt buộc")]
    [Display(Name = "Vai trò")]
    public int RoleId { get; set; }

    [Display(Name = "Trạng thái hoạt động")]
    public bool IsActive { get; set; }
}

public class UserIndexViewModel
{
    public IEnumerable<UserListViewModel> Users { get; set; } = new List<UserListViewModel>();
    public int TotalUsers { get; set; }
    public int InstructorCount { get; set; }
    public int StudentCount { get; set; }
    public string? SearchQuery { get; set; }
    public string? SelectedRole { get; set; }
    
    // Pagination properties / Thuộc tính phân trang
    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int TotalPages { get; set; }
    
    // Computed properties for pagination navigation / Thuộc tính tính toán cho điều hướng phân trang
    public bool HasPreviousPage => CurrentPage > 1;
    public bool HasNextPage => CurrentPage < TotalPages;
}


// ViewModel for User Details with Courses
public class UserDetailsViewModel
{
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Danh sách khóa học cho Instructor
    public List<UserCourseViewModel> Courses { get; set; } = new();
    public int TotalStudents { get; set; }
    public int TotalLessons { get; set; }
    
    // Danh sách khóa học đã đăng ký cho Student
    public List<UserEnrollmentViewModel> Enrollments { get; set; } = new();
    
    // Các thuộc tính tính toán cho Student
    public int TotalEnrolledCourses => Enrollments.Count;
    public int CompletedCoursesCount => Enrollments.Count(e => e.LearningStatus == Online_Course.Models.LearningStatus.COMPLETED);
    public double AverageProgress => Enrollments.Any() ? Enrollments.Average(e => e.ProgressPercent) : 0;
}

public class UserCourseViewModel
{
    public int CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int? CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public Online_Course.Models.CourseStatus Status { get; set; } = Online_Course.Models.CourseStatus.Draft;
    public int EnrollmentCount { get; set; }
    public int LessonCount { get; set; }
    
    public string StatusText => Status switch
    {
        Online_Course.Models.CourseStatus.Public => "Công khai",
        Online_Course.Models.CourseStatus.Private => "Riêng tư",
        _ => "Nháp"
    };
}

// ViewModel for Student Enrollments
public class UserEnrollmentViewModel
{
    public int CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string ThumbnailUrl { get; set; } = string.Empty;
    public DateTime EnrolledAt { get; set; }
    public Online_Course.Models.LearningStatus LearningStatus { get; set; }
    public float ProgressPercent { get; set; }
    public int CompletedLessons { get; set; }
    public int TotalLessons { get; set; }
    
    // Thuộc tính hiển thị trạng thái học tập
    public string StatusText => LearningStatus switch
    {
        Online_Course.Models.LearningStatus.NOT_STARTED => "Chưa học",
        Online_Course.Models.LearningStatus.IN_PROGRESS => "Đang học",
        Online_Course.Models.LearningStatus.COMPLETED => "Hoàn thành",
        _ => "Không xác định"
    };
    
    // Thuộc tính định dạng % tiến độ
    public string ProgressText => $"{ProgressPercent:F1}%";
}

// ViewModels for Forgot Password
public class ForgotPasswordViewModel
{
    [Required(ErrorMessage = "Email là bắt buộc")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    public string Email { get; set; } = string.Empty;
}

public class ResetPasswordViewModel
{
    [Required]
    public string Token { get; set; } = string.Empty;

    [Required]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Mật khẩu mới là bắt buộc")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
    [DataType(DataType.Password)]
    [Display(Name = "Mật khẩu mới")]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Xác nhận mật khẩu là bắt buộc")]
    [DataType(DataType.Password)]
    [Compare("NewPassword", ErrorMessage = "Mật khẩu xác nhận không khớp")]
    [Display(Name = "Xác nhận mật khẩu")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
