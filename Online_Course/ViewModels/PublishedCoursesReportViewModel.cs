using Online_Course.Models;

namespace Online_Course.ViewModels;

public class PublishedCoursesReportViewModel
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string MonthName { get; set; } = string.Empty;
    public List<Course> Courses { get; set; } = new List<Course>();
}
