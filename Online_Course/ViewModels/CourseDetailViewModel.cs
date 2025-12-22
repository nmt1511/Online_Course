using Online_Course.Models;

namespace Online_Course.ViewModels;

/// <summary>
/// ViewModel for course detail page - used by both Admin and Instructor
/// </summary>
public class CourseDetailViewModel
{
    public int CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int? CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string ThumbnailUrl { get; set; } = string.Empty;
    public CourseStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Course Type and Date fields
    public CourseType CourseType { get; set; } = CourseType.Open_Always;
    public DateTime? RegistrationStartDate { get; set; }
    public DateTime? RegistrationEndDate { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    
    // Only Admin can see this
    public string? InstructorName { get; set; }
    public bool ShowInstructor { get; set; }
    
    // Statistics
    public int TotalLessons { get; set; }
    public int TotalStudents { get; set; }
    
    public IEnumerable<LessonSummaryViewModel> Lessons { get; set; } = new List<LessonSummaryViewModel>();
    public IEnumerable<StudentEnrollmentViewModel> Students { get; set; } = new List<StudentEnrollmentViewModel>();
    
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
        _ => "bg-gray-500"
    };
    
    public string CourseTypeText => CourseType switch
    {
        CourseType.Fixed_Time => "Thời gian cố định",
        _ => "Mở xuyên suốt"
    };
    
    public string CourseTypeIcon => CourseType switch
    {
        CourseType.Fixed_Time => "schedule",
        _ => "all_inclusive"
    };
    
    public bool IsFixedTime => CourseType == CourseType.Fixed_Time;
}


public class LessonSummaryViewModel
{
    public int LessonId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int OrderIndex { get; set; }
    
    // Lesson content URL (YouTube video or PDF link)
    public string ContentUrl { get; set; } = string.Empty;
    
    // Lesson type: "video" or "pdf"
    public LessonType LessonType { get; set; } = LessonType.Video;
    
    public string LessonTypeIcon => LessonType switch
    {
        LessonType.Pdf => "picture_as_pdf",
        _ => "play_circle"
    };
    
    public string LessonTypeText => LessonType switch
    {
        LessonType.Pdf => "PDF",
        _ => "Video"
    };
}

public class StudentEnrollmentViewModel
{
    public int StudentId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime EnrolledAt { get; set; }
    
    // Completion percentage (0-100)
    public double CompletionPercentage { get; set; }
    
    public string CompletionColor => CompletionPercentage switch
    {
        100 => "bg-green-500",
        >= 50 => "bg-blue-500",
        > 0 => "bg-yellow-500",
        _ => "bg-gray-300"
    };
}
