namespace Online_Course.Models;

public enum CourseStatus
{
    Draft,      // Nháp
    Private,    // Riêng tư
    Public,     // Công khai
    Closed      // Đã đóng
}

public enum CourseType
{
    Fixed_Time, //thời gian cố định
    Open_Always //mở xuyên suốt
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
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
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
