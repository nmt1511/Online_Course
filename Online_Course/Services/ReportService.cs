using Microsoft.EntityFrameworkCore;
using Online_Course.Data;
using Online_Course.Models;

namespace Online_Course.Services;

public class ReportService : IReportService
{
    private readonly ApplicationDbContext _context;

    public ReportService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> GetMonthlyPublishedCoursesCountAsync(int year, int month)
    {
        // Since Course doesn't have CreatedAt, we count all published courses
        // In a real scenario, you'd add a CreatedAt field to Course
        return await _context.Courses
            .Where(c => c.Status == CourseStatus.Public)
            .CountAsync();
    }

    public async Task<IEnumerable<CoursePopularityReport>> GetPopularCoursesAsync(int topCount = 10)
    {
        return await _context.Courses
            .Where(c => c.Status == CourseStatus.Public)
            .Select(c => new CoursePopularityReport
            {
                CourseId = c.CourseId,
                Title = c.Title,
                Category = c.Category,
                InstructorName = c.Instructor.FullName,
                EnrollmentCount = c.Enrollments.Count
            })
            .OrderByDescending(c => c.EnrollmentCount)
            .Take(topCount)
            .ToListAsync();
    }

    public async Task<int> GetMonthlyEnrollmentsCountAsync(int year, int month)
    {
        var startDate = new DateTime(year, month, 1);
        var endDate = startDate.AddMonths(1);

        return await _context.Enrollments
            .Where(e => e.EnrolledAt >= startDate && e.EnrolledAt < endDate)
            .CountAsync();
    }


    public async Task<int> GetMonthlyActiveInstructorsCountAsync(int year, int month)
    {
        // Active instructors are those who have at least one published course
        return await _context.Courses
            .Where(c => c.Status == CourseStatus.Public)
            .Select(c => c.CreatedBy)
            .Distinct()
            .CountAsync();
    }

    public async Task<MonthlyStatistics> GetMonthlyStatisticsAsync(int year, int month)
    {
        var startDate = new DateTime(year, month, 1);
        var endDate = startDate.AddMonths(1);

        var publishedCourses = await _context.Courses
            .Where(c => c.Status == CourseStatus.Public)
            .CountAsync();

        var totalEnrollments = await _context.Enrollments
            .Where(e => e.EnrolledAt >= startDate && e.EnrolledAt < endDate)
            .CountAsync();

        var activeInstructors = await _context.Courses
            .Where(c => c.Status == CourseStatus.Public)
            .Select(c => c.CreatedBy)
            .Distinct()
            .CountAsync();

        var newStudents = await _context.Users
            .Where(u => u.CreatedAt >= startDate && u.CreatedAt < endDate)
            .Where(u => u.UserRoles.Any(ur => ur.Role.Name == "Student"))
            .CountAsync();

        return new MonthlyStatistics
        {
            Year = year,
            Month = month,
            PublishedCourses = publishedCourses,
            TotalEnrollments = totalEnrollments,
            ActiveInstructors = activeInstructors,
            NewStudents = newStudents
        };
    }

    public async Task<IEnumerable<MonthlyTrend>> GetEnrollmentTrendsAsync(int months = 6)
    {
        var trends = new List<MonthlyTrend>();
        var currentDate = DateTime.UtcNow;
        var vietnameseCulture = new System.Globalization.CultureInfo("vi-VN");

        for (int i = months - 1; i >= 0; i--)
        {
            var targetDate = currentDate.AddMonths(-i);
            var year = targetDate.Year;
            var month = targetDate.Month;
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1);

            var count = await _context.Enrollments
                .Where(e => e.EnrolledAt >= startDate && e.EnrolledAt < endDate)
                .CountAsync();

            trends.Add(new MonthlyTrend
            {
                Year = year,
                Month = month,
                MonthName = $"Tháng {month}/{year}",
                Count = count
            });
        }

        return trends;
    }

    public async Task<IEnumerable<MonthlyTrend>> GetCoursePublicationTrendsAsync(int months = 6)
    {
        var trends = new List<MonthlyTrend>();
        var currentDate = DateTime.UtcNow;

        // Since Course doesn't have CreatedAt, we'll return the total published courses
        // divided evenly for demonstration purposes
        var totalPublished = await _context.Courses.Where(c => c.Status == CourseStatus.Public).CountAsync();

        for (int i = months - 1; i >= 0; i--)
        {
            var targetDate = currentDate.AddMonths(-i);
            var year = targetDate.Year;
            var month = targetDate.Month;
            var startDate = new DateTime(year, month, 1);

            trends.Add(new MonthlyTrend
            {
                Year = year,
                Month = month,
                MonthName = startDate.ToString("MMM yyyy"),
                Count = i == 0 ? totalPublished : 0 // Show total only in current month
            });
        }

        return trends;
    }
}
