using Online_Course.Models;

namespace Online_Course.Services;

public interface IReportService
{
    /// <summary>
    /// Gets the count of courses published in a specific month
    /// </summary>
    Task<int> GetMonthlyPublishedCoursesCountAsync(int year, int month);
    
    /// <summary>
    /// Gets the most popular courses by enrollment count
    /// </summary>
    Task<IEnumerable<CoursePopularityReport>> GetPopularCoursesAsync(int topCount = 10);
    
    /// <summary>
    /// Gets the count of enrollments in a specific month
    /// </summary>
    Task<int> GetMonthlyEnrollmentsCountAsync(int year, int month);
    
    /// <summary>
    /// Gets the count of active instructors (instructors with at least one course) in a specific month
    /// </summary>
    Task<int> GetMonthlyActiveInstructorsCountAsync(int year, int month);
    
    /// <summary>
    /// Gets monthly statistics for the dashboard
    /// </summary>
    Task<MonthlyStatistics> GetMonthlyStatisticsAsync(int year, int month);
    
    /// <summary>
    /// Gets enrollment trends for the last N months
    /// </summary>
    Task<IEnumerable<MonthlyTrend>> GetEnrollmentTrendsAsync(int months = 6);
    
    /// <summary>
    /// Gets course publication trends for the last N months
    /// </summary>
    Task<IEnumerable<MonthlyTrend>> GetCoursePublicationTrendsAsync(int months = 6);
}

/// <summary>
/// Report model for popular courses
/// </summary>
public class CoursePopularityReport
{
    public int CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int? CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string InstructorName { get; set; } = string.Empty;
    public int EnrollmentCount { get; set; }
}

/// <summary>
/// Monthly statistics summary
/// </summary>
public class MonthlyStatistics
{
    public int Year { get; set; }
    public int Month { get; set; }
    public int PublishedCourses { get; set; }
    public int TotalEnrollments { get; set; }
    public int ActiveInstructors { get; set; }
    public int NewStudents { get; set; }
}

/// <summary>
/// Monthly trend data point
/// </summary>
public class MonthlyTrend
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string MonthName { get; set; } = string.Empty;
    public int Count { get; set; }
}
