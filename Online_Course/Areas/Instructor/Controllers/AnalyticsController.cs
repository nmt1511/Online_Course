using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Online_Course.Services;
using System.Security.Claims;

namespace Online_Course.Areas.Instructor.Controllers;

[Area("Instructor")]
[Authorize(Roles = "Instructor")]
public class AnalyticsController : Controller
{
    private readonly ICourseService _courseService;
    private readonly IEnrollmentService _enrollmentService;
    private readonly IProgressService _progressService;

    public AnalyticsController(
        ICourseService courseService,
        IEnrollmentService enrollmentService,
        IProgressService progressService)
    {
        _courseService = courseService;
        _enrollmentService = enrollmentService;
        _progressService = progressService;
    }

    public async Task<IActionResult> Index()
    {
        var instructorId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        var coursesEnumerable = await _courseService.GetCoursesByInstructorAsync(instructorId);
        var courses = coursesEnumerable.ToList();

        var totalStudents = 0;
        var totalLessons = 0;
        var totalCompletions = 0;
        var courseStats = new List<CourseAnalyticsViewModel>();

        foreach (var course in courses)
        {
            var enrollmentsEnumerable = await _enrollmentService.GetEnrollmentsByCourseAsync(course.CourseId);
            var enrollments = enrollmentsEnumerable.ToList();
            var lessonCount = course.Lessons?.Count ?? 0;
            totalLessons += lessonCount;
            totalStudents += enrollments.Count;

            double avgProgress = 0;
            int completedStudents = 0;

            foreach (var enrollment in enrollments)
            {
                var progress = await _progressService.CalculateProgressPercentageAsync(
                    enrollment.StudentId, course.CourseId);
                avgProgress += progress;
                if (progress >= 100) completedStudents++;
            }

            totalCompletions += completedStudents;
            avgProgress = enrollments.Count > 0 ? avgProgress / enrollments.Count : 0;

            courseStats.Add(new CourseAnalyticsViewModel
            {
                CourseId = course.CourseId,
                Title = course.Title,
                CategoryId = course.CategoryId,
                CategoryName = course.CategoryEntity?.Name ?? "Chưa phân loại",
                TotalStudents = enrollments.Count,
                TotalLessons = lessonCount,
                AverageProgress = Math.Round(avgProgress, 1),
                CompletedStudents = completedStudents,
                Status = course.CourseStatus
            });
        }

        var viewModel = new InstructorAnalyticsViewModel
        {
            TotalCourses = courses.Count,
            TotalStudents = totalStudents,
            TotalLessons = totalLessons,
            TotalCompletions = totalCompletions,
            AverageCompletionRate = totalStudents > 0 ? Math.Round((double)totalCompletions / totalStudents * 100, 1) : 0,
            Courses = courseStats.OrderByDescending(c => c.TotalStudents).ToList()
        };

        return View(viewModel);
    }
}

public class CourseAnalyticsViewModel
{
    public int CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int? CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int TotalStudents { get; set; }
    public int TotalLessons { get; set; }
    public double AverageProgress { get; set; }
    public int CompletedStudents { get; set; }
    public Online_Course.Models.CourseStatus Status { get; set; }
    
    public string StatusText => Status switch
    {
        Online_Course.Models.CourseStatus.Public => "Công khai",
        Online_Course.Models.CourseStatus.Private => "Riêng tư",
        _ => "Nháp"
    };
}

public class InstructorAnalyticsViewModel
{
    public int TotalCourses { get; set; }
    public int TotalStudents { get; set; }
    public int TotalLessons { get; set; }
    public int TotalCompletions { get; set; }
    public double AverageCompletionRate { get; set; }
    public List<CourseAnalyticsViewModel> Courses { get; set; } = new();
}
