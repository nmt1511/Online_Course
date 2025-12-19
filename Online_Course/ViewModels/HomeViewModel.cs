namespace Online_Course.ViewModels;

public class HomeViewModel
{
    public List<HomeCourseViewModel> FeaturedCourses { get; set; } = new();
    public List<HomeCategoryViewModel> Categories { get; set; } = new();
    public int TotalCourses { get; set; }
    public int TotalCategories { get; set; }
}

public class HomeCourseViewModel
{
    public int CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string InstructorName { get; set; } = string.Empty;
    public int LessonCount { get; set; }
}

public class HomeCategoryViewModel
{
    public int CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Slug { get; set; }
}
