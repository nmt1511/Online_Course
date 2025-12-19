namespace Online_Course.Models;

public enum LearningStatus
{
    NOT_STARTED,    //chưa bắt đầu
    IN_PROGRESS,    // đang học
    COMPLETED       // hoàn thành
}
public class Enrollment
{
    public int EnrollmentId { get; set; }
    public int CourseId { get; set; }
    public int StudentId { get; set; }
    public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;
    public LearningStatus LearningStatus { get; set; } = LearningStatus.NOT_STARTED;
    public float ProgressPercent { get; set; }
    public bool IsMandatory { get; set; } = false;
    public Course Course { get; set; } = null!;
    public User Student { get; set; } = null!;
}
