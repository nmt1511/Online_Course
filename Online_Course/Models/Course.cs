namespace Online_Course.Models;

public enum CourseStatus
{
    Draft,      // Nháp
    Private,    // Riêng tư
    Public      // Công khai
}

public class Course
{
    public int CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string ThumbnailUrl { get; set; } = string.Empty;
    public int CreatedBy { get; set; }
    public CourseStatus Status { get; set; } = CourseStatus.Draft;
    public int? CategoryId { get; set; }
    public Category? CategoryEntity { get; set; }
    public User Instructor { get; set; } = null!;
    public ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}
