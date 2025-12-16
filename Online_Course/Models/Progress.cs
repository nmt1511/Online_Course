namespace Online_Course.Models;

public class Progress
{
    public int ProgressId { get; set; }
    public int LessonId { get; set; }
    public int StudentId { get; set; }
    public bool IsCompleted { get; set; } = false;
    public DateTime LastUpdate { get; set; } = DateTime.UtcNow;
    public Lesson Lesson { get; set; } = null!;
    public User Student { get; set; } = null!;
}
