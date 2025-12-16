namespace Online_Course.ViewModels;

public class StudentProgressViewModel
{
    public int StudentId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime EnrolledAt { get; set; }
    public double ProgressPercentage { get; set; }
    public int CompletedLessons { get; set; }
    public int TotalLessons { get; set; }
    public string Status { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
    public int CourseId { get; set; }
}

public class CourseStudentsViewModel
{
    public int CourseId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public int TotalStudents { get; set; }
    public double AverageProgress { get; set; }
    public int CompletedCount { get; set; }
    public List<StudentProgressViewModel> Students { get; set; } = new();
}

// ViewModels for Student Progress Tracking page
public class StudentCourseProgressViewModel
{
    public int CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string ThumbnailUrl { get; set; } = string.Empty;
    public int TotalLessons { get; set; }
    public int CompletedLessons { get; set; }
    public double ProgressPercentage { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime EnrolledAt { get; set; }
    public string CurrentLessonTitle { get; set; } = string.Empty;
}

public class StudentProgressIndexViewModel
{
    public int TotalCourses { get; set; }
    public int CompletedCourses { get; set; }
    public double OverallProgress { get; set; }
    public List<StudentCourseProgressViewModel> Courses { get; set; } = new();
}
