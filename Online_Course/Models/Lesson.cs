namespace Online_Course.Models;

public enum LessonType
{
    Pdf,      // nội dung pdf
    Video     // nội dung video
}
public class Lesson
{
    public int LessonId { get; set; }
    public int CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ContentUrl { get; set; } = string.Empty;
    public LessonType LessonType { get; set; } = LessonType.Video;
    public int? TotalDurationSeconds { get; set; } // Tổng thời gian của video (tính bằng giây)
    public int? TotalPages { get; set; } // Tổng số trang của tài liệu PDF
    public int OrderIndex { get; set; } //thứ tự bài học trong khóa học
    public Course Course { get; set; } = null!;
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public ICollection<Progress> Progresses { get; set; } = new List<Progress>();
}
