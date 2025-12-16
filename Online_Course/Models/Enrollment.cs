namespace Online_Course.Models;

public class Enrollment
{
    public int EnrollmentId { get; set; }
    public int CourseId { get; set; }
    public int StudentId { get; set; }
    public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;
    public Course Course { get; set; } = null!;
    public User Student { get; set; } = null!;
}
