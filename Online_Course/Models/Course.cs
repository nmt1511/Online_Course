using Online_Course.Helper;

namespace Online_Course.Models;

public enum CourseStatus
{
    Draft,      // Trạng thái nháp
    Private,    // Trạng thái riêng tư
    Public,     // Trạng thái công khai
    Closed      // Trạng thái đã đóng
}

public enum CourseType
{
    Fixed_Time, // Khóa học có thời hạn cố định
    Open_Always // Khóa học mở xuyên suốt thời gian
}

public class Course
{
    public int CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ThumbnailUrl { get; set; } = string.Empty;
    public int CreatedBy { get; set; }
    public CourseStatus CourseStatus { get; set; } = CourseStatus.Draft;
    public CourseType CourseType { get; set; } = CourseType.Open_Always;
    public DateTime CreatedAt { get; set; } = DateTimeHelper.GetVietnamTimeNow();
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime? RegistrationStartDate { get; set; }
    public DateTime? RegistrationEndDate { get; set; }
    public int? CategoryId { get; set; }
    public Category? CategoryEntity { get; set; }
    public User Instructor { get; set; } = null!;
    public ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}
