namespace Online_Course.Models;

public class Lesson
{
    public int LessonId { get; set; }
    public int CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string VideoUrl { get; set; } = string.Empty;
    public int OrderIndex { get; set; }
    public Course Course { get; set; } = null!;
    public ICollection<Progress> Progresses { get; set; } = new List<Progress>();
}
