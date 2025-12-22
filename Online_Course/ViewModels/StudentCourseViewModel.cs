using Online_Course.Models;

namespace Online_Course.ViewModels;

public class StudentCourseListViewModel
{
    public int CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int? CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string ThumbnailUrl { get; set; } = string.Empty;
    public string InstructorName { get; set; } = string.Empty;
    public int EnrollmentCount { get; set; }
    public int LessonCount { get; set; }
    public CourseType CourseType { get; set; }
}

public class StudentCourseIndexViewModel
{
    public IList<StudentCourseListViewModel> Courses { get; set; } = new List<StudentCourseListViewModel>();
    public IList<Category> Categories { get; set; } = new List<Category>();
    public int? SelectedCategoryId { get; set; }
    public CourseType? SelectedCourseType { get; set; }
}

public class StudentCourseDetailsViewModel
{
    public int CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int? CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string ThumbnailUrl { get; set; } = string.Empty;
    public string InstructorName { get; set; } = string.Empty;
    public int EnrollmentCount { get; set; }
    public IList<StudentLessonViewModel> Lessons { get; set; } = new List<StudentLessonViewModel>();
    public bool IsEnrolled { get; set; }
    public CourseType CourseType { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime? RegistrationStartDate { get; set; }
    public DateTime? RegistrationEndDate { get; set; }
}

public class StudentLessonViewModel
{
    public int LessonId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int OrderIndex { get; set; }
}
