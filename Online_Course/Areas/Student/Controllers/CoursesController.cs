using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Online_Course.Models;
using Online_Course.Services;
using Online_Course.ViewModels;

namespace Online_Course.Areas.Student.Controllers;

[Area("Student")]
public class CoursesController : Controller
{
    private readonly ICourseService _courseService;
    private readonly ICategoryService _categoryService;
    private readonly IEnrollmentService _enrollmentService;

    public CoursesController(
        ICourseService courseService,
        ICategoryService categoryService,
        IEnrollmentService enrollmentService)
    {
        _courseService = courseService;
        _categoryService = categoryService;
        _enrollmentService = enrollmentService;
    }

    [HttpPost]
    [Authorize(Roles = "Student")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Enroll(int courseId)
    {
        var userIdClaim = User.FindFirst("UserId")?.Value;
        if (!int.TryParse(userIdClaim, out int userId))
        {
            return RedirectToAction("Login", "Account", new { area = "" });
        }

        // Check if course exists and is published
        var course = await _courseService.GetCourseByIdAsync(courseId);
        if (course == null || course.Status != CourseStatus.Public)
        {
            TempData["Error"] = "Khóa học không tồn tại hoặc chưa được xuất bản.";
            return RedirectToAction(nameof(Index));
        }

        // Check if already enrolled
        var isEnrolled = await _enrollmentService.IsEnrolledAsync(userId, courseId);
        if (isEnrolled)
        {
            TempData["Info"] = "Bạn đã đăng ký khóa học này rồi.";
            return RedirectToAction(nameof(Details), new { id = courseId });
        }

        // Create enrollment
        await _enrollmentService.EnrollAsync(userId, courseId);
        TempData["Success"] = "Đăng ký khóa học thành công!";

        // Redirect to lesson list (Learning area)
        return RedirectToAction("Lessons", "Learning", new { area = "Student", courseId = courseId });
    }

    [AllowAnonymous]
    public async Task<IActionResult> Index(string? category)
    {
        var courses = string.IsNullOrEmpty(category)
            ? await _courseService.GetAllCoursesAsync()
            : await _courseService.GetCoursesByCategoryAsync(category);

        // Only show published courses to students
        var publishedCourses = courses.Where(c => c.Status == CourseStatus.Public).ToList();

        var categories = await _categoryService.GetAllCategoriesAsync();

        var viewModel = new StudentCourseIndexViewModel
        {
            Courses = publishedCourses.Select(c => new StudentCourseListViewModel
            {
                CourseId = c.CourseId,
                Title = c.Title,
                Description = c.Description,
                Category = c.Category,
                ThumbnailUrl = c.ThumbnailUrl,
                InstructorName = c.Instructor?.FullName ?? "Unknown",
                EnrollmentCount = c.Enrollments?.Count ?? 0,
                LessonCount = c.Lessons?.Count ?? 0
            }).ToList(),
            Categories = categories.Where(c => c.IsActive).Select(c => c.Name).ToList(),
            SelectedCategory = category
        };

        return View(viewModel);
    }

    [AllowAnonymous]
    public async Task<IActionResult> Details(int id)
    {
        var course = await _courseService.GetCourseByIdAsync(id);
        
        if (course == null || course.Status != CourseStatus.Public)
        {
            return NotFound();
        }

        var isEnrolled = false;
        if (User.Identity?.IsAuthenticated == true)
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (int.TryParse(userIdClaim, out int userId))
            {
                isEnrolled = await _enrollmentService.IsEnrolledAsync(userId, id);
            }
        }

        var viewModel = new StudentCourseDetailsViewModel
        {
            CourseId = course.CourseId,
            Title = course.Title,
            Description = course.Description,
            Category = course.Category,
            ThumbnailUrl = course.ThumbnailUrl,
            InstructorName = course.Instructor?.FullName ?? "Unknown",
            EnrollmentCount = course.Enrollments?.Count ?? 0,
            Lessons = course.Lessons?.OrderBy(l => l.OrderIndex).Select(l => new StudentLessonViewModel
            {
                LessonId = l.LessonId,
                Title = l.Title,
                Description = l.Description,
                OrderIndex = l.OrderIndex
            }).ToList() ?? new List<StudentLessonViewModel>(),
            IsEnrolled = isEnrolled
        };

        return View(viewModel);
    }

    [HttpPost]
    [Authorize(Roles = "Student")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Unenroll(int courseId)
    {
        var userIdClaim = User.FindFirst("UserId")?.Value;
        if (!int.TryParse(userIdClaim, out int userId))
        {
            return RedirectToAction("Login", "Account", new { area = "" });
        }

        // Check if enrolled
        var isEnrolled = await _enrollmentService.IsEnrolledAsync(userId, courseId);
        if (!isEnrolled)
        {
            TempData["Error"] = "Bạn chưa đăng ký khóa học này.";
            return RedirectToAction("Index", "Progress", new { area = "Student" });
        }

        // Unenroll
        var result = await _enrollmentService.UnenrollAsync(userId, courseId);
        if (result)
        {
            TempData["Success"] = "Hủy đăng ký khóa học thành công!";
        }
        else
        {
            TempData["Error"] = "Có lỗi xảy ra khi hủy đăng ký.";
        }

        return RedirectToAction("Index", "Progress", new { area = "Student" });
    }
}
