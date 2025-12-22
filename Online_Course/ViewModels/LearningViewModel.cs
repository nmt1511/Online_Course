namespace Online_Course.ViewModels;

public class LearningLessonViewModel
{
    public int LessonId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string VideoUrl { get; set; } = string.Empty;
    public int OrderIndex { get; set; }
    public bool IsCompleted { get; set; }
}

public class LearningLessonsViewModel
{
    public int CourseId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public int? CourseCategoryId { get; set; }
    public string CourseCategoryName { get; set; } = string.Empty;
    public int TotalLessons { get; set; }
    public int CompletedLessons { get; set; }
    public double ProgressPercentage { get; set; }
    public IList<LearningLessonViewModel> Lessons { get; set; } = new List<LearningLessonViewModel>();
}

public class LearningContentViewModel
{
    public int LessonId { get; set; }
    public string LessonTitle { get; set; } = string.Empty;
    public string LessonDescription { get; set; } = string.Empty;
    public string VideoUrl { get; set; } = string.Empty;
    public int OrderIndex { get; set; }
    public bool IsCompleted { get; set; }
    
    public int CourseId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public int? CourseCategoryId { get; set; }
    public string CourseCategoryName { get; set; } = string.Empty;
    public string InstructorName { get; set; } = string.Empty;
    
    public int TotalLessons { get; set; }
    public int CompletedLessons { get; set; }
    public double ProgressPercentage { get; set; }
    
    public int? PreviousLessonId { get; set; }
    public int? NextLessonId { get; set; }
    
    public IList<LearningLessonViewModel> AllLessons { get; set; } = new List<LearningLessonViewModel>();
}
