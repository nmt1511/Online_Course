namespace Online_Course.ViewModels;

public class StudentCourseListViewModel
{
    public int CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string ThumbnailUrl { get; set; } = string.Empty;
    public string InstructorName { get; set; } = string.Empty;
    public int EnrollmentCount { get; set; }
    public int LessonCount { get; set; }
}

public class StudentCourseIndexViewModel
{
    public IList<StudentCourseListViewModel> Courses { get; set; } = new List<StudentCourseListViewModel>();
    public IList<string> Categories { get; set; } = new List<string>();
    public string? SelectedCategory { get; set; }
}

public class StudentCourseDetailsViewModel
{
    public int CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string ThumbnailUrl { get; set; } = string.Empty;
    public string InstructorName { get; set; } = string.Empty;
    public int EnrollmentCount { get; set; }
    public IList<StudentLessonViewModel> Lessons { get; set; } = new List<StudentLessonViewModel>();
    public bool IsEnrolled { get; set; }
}

public class StudentLessonViewModel
{
    public int LessonId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int OrderIndex { get; set; }
}
