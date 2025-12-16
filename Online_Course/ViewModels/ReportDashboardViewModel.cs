using Online_Course.Services;

namespace Online_Course.ViewModels;

public class ReportDashboardViewModel
{
    public string CurrentMonth { get; set; } = string.Empty;
    public MonthlyStatistics MonthlyStatistics { get; set; } = new();
    public List<CoursePopularityReport> PopularCourses { get; set; } = new();
    public List<MonthlyTrend> EnrollmentTrends { get; set; } = new();
    public int TotalCourses { get; set; }
    public int TotalUsers { get; set; }
    public int TotalStudents { get; set; }
    public int TotalInstructors { get; set; }
}
