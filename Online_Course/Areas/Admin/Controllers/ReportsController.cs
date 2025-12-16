using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Online_Course.Services;
using Online_Course.ViewModels;

namespace Online_Course.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "AdminOnly")]
public class ReportsController : Controller
{
    private readonly IReportService _reportService;
    private readonly ICourseService _courseService;
    private readonly IUserService _userService;
    private readonly IEnrollmentService _enrollmentService;

    public ReportsController(
        IReportService reportService,
        ICourseService courseService,
        IUserService userService,
        IEnrollmentService enrollmentService)
    {
        _reportService = reportService;
        _courseService = courseService;
        _userService = userService;
        _enrollmentService = enrollmentService;
    }

    public async Task<IActionResult> Index()
    {
        var currentDate = DateTime.UtcNow;
        var year = currentDate.Year;
        var month = currentDate.Month;

        var statistics = await _reportService.GetMonthlyStatisticsAsync(year, month);
        var popularCourses = await _reportService.GetPopularCoursesAsync(5);
        var enrollmentTrends = await _reportService.GetEnrollmentTrendsAsync(6);

        // Get overall statistics
        var totalCourses = await _courseService.GetTotalCoursesCountAsync();
        var totalUsers = await _userService.GetTotalUsersCountAsync();
        var totalStudents = await _userService.GetUserCountByRoleAsync("Student");
        var totalInstructors = await _userService.GetUserCountByRoleAsync("Instructor");

        var viewModel = new ReportDashboardViewModel
        {
            CurrentMonth = currentDate.ToString("MMMM yyyy"),
            MonthlyStatistics = statistics,
            PopularCourses = popularCourses.ToList(),
            EnrollmentTrends = enrollmentTrends.ToList(),
            TotalCourses = totalCourses,
            TotalUsers = totalUsers,
            TotalStudents = totalStudents,
            TotalInstructors = totalInstructors
        };

        return View(viewModel);
    }


    [HttpGet]
    public async Task<IActionResult> MonthlyPublishedCourses(int? year, int? month)
    {
        var targetYear = year ?? DateTime.UtcNow.Year;
        var targetMonth = month ?? DateTime.UtcNow.Month;

        var count = await _reportService.GetMonthlyPublishedCoursesCountAsync(targetYear, targetMonth);

        return Json(new { year = targetYear, month = targetMonth, count });
    }

    [HttpGet]
    public async Task<IActionResult> PopularCourses(int topCount = 10)
    {
        var courses = await _reportService.GetPopularCoursesAsync(topCount);
        return Json(courses);
    }

    [HttpGet]
    public async Task<IActionResult> MonthlyEnrollments(int? year, int? month)
    {
        var targetYear = year ?? DateTime.UtcNow.Year;
        var targetMonth = month ?? DateTime.UtcNow.Month;

        var count = await _reportService.GetMonthlyEnrollmentsCountAsync(targetYear, targetMonth);

        return Json(new { year = targetYear, month = targetMonth, count });
    }

    [HttpGet]
    public async Task<IActionResult> MonthlyActiveInstructors(int? year, int? month)
    {
        var targetYear = year ?? DateTime.UtcNow.Year;
        var targetMonth = month ?? DateTime.UtcNow.Month;

        var count = await _reportService.GetMonthlyActiveInstructorsCountAsync(targetYear, targetMonth);

        return Json(new { year = targetYear, month = targetMonth, count });
    }

    [HttpGet]
    public async Task<IActionResult> EnrollmentTrends(int months = 6)
    {
        var trends = await _reportService.GetEnrollmentTrendsAsync(months);
        return Json(trends);
    }
}
