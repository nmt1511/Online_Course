using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Online_Course.Services;
using Online_Course.ViewModels;
using System.Security.Claims;

namespace Online_Course.Areas.Instructor.Controllers;

[Area("Instructor")]
[Authorize(Policy = "InstructorOnly")]
public class DashboardController : Controller
{
    private readonly ICourseService _courseService;
    private readonly IEnrollmentService _enrollmentService;
    private readonly IUserService _userService;

    public DashboardController(
        ICourseService courseService,
        IEnrollmentService enrollmentService,
        IUserService userService)
    {
        _courseService = courseService;
        _enrollmentService = enrollmentService;
        _userService = userService;
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        return userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;
    }

    public async Task<IActionResult> Index()
    {
        var instructorId = GetCurrentUserId();
        var instructor = await _userService.GetUserByIdAsync(instructorId);
        var courses = await _courseService.GetCoursesByInstructorAsync(instructorId);

        var totalStudents = courses.Sum(c => c.Enrollments?.Count ?? 0);
        var activeCourses = courses.Count(c => c.CourseStatus == Models.CourseStatus.Public);
        var draftCourses = courses.Count(c => c.CourseStatus == Models.CourseStatus.Draft);

        var viewModel = new InstructorDashboardViewModel
        {
            InstructorName = instructor?.FullName ?? "Instructor",
            TotalStudents = totalStudents,
            ActiveCourses = activeCourses,
            DraftCourses = draftCourses,
            AverageCompletion = 76, // Placeholder
            Courses = courses.Select(c => new InstructorCourseListViewModel
            {
                CourseId = c.CourseId,
                Title = c.Title,
                Description = c.Description,
                CategoryId = c.CategoryId,
                CategoryName = c.CategoryEntity?.Name ?? "Chưa phân loại",
                ThumbnailUrl = c.ThumbnailUrl,
                Status = c.CourseStatus,
                EnrollmentCount = c.Enrollments?.Count ?? 0,
                LessonCount = c.Lessons?.Count ?? 0,
                AverageRating = 4.5
            }).ToList()
        };

        return View(viewModel);
    }
}
