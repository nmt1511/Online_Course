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
    public int OrderIndex { get; set; }
    public Course Course { get; set; } = null!;
    public ICollection<Progress> Progresses { get; set; } = new List<Progress>();
}
