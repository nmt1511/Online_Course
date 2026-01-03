using Online_Course.Helper;

namespace Online_Course.Models;

public enum LearningStatus
{
    NOT_STARTED,    // Trạng thái chưa bắt đầu học
    IN_PROGRESS,    // Trạng thái đang học
    COMPLETED       // Trạng thái đã hoàn thành khóa học
}
public class Enrollment
{
    public int EnrollmentId { get; set; }
    public int CourseId { get; set; }
    public int StudentId { get; set; }
    public DateTime EnrolledAt { get; set; } = DateTimeHelper.GetVietnamTimeNow();
    public LearningStatus LearningStatus { get; set; } = LearningStatus.NOT_STARTED;
    public float ProgressPercent { get; set; }
    public bool IsMandatory { get; set; } = false;
    public Course Course { get; set; } = null!;
    public User Student { get; set; } = null!;
}
