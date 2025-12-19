using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Online_Course.Services;
using Online_Course.ViewModels;

namespace Online_Course.Areas.Student.Controllers;

[Area("Student")]
[Authorize(Roles = "Student")]
public class LearningController : Controller
{
    private readonly ICourseService _courseService;
    private readonly ILessonService _lessonService;
    private readonly IEnrollmentService _enrollmentService;
    private readonly IProgressService _progressService;

    public LearningController(
        ICourseService courseService,
        ILessonService lessonService,
        IEnrollmentService enrollmentService,
        IProgressService progressService)
    {
        _courseService = courseService;
        _lessonService = lessonService;
        _enrollmentService = enrollmentService;
        _progressService = progressService;
    }

    private int? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("UserId")?.Value;
        if (int.TryParse(userIdClaim, out int userId))
            return userId;
        return null;
    }

    // GET: Student/Learning/Lessons/{courseId}
    public async Task<IActionResult> Lessons(int courseId)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return RedirectToAction("Login", "Account", new { area = "" });

        // Check enrollment
        var isEnrolled = await _enrollmentService.IsEnrolledAsync(userId.Value, courseId);
        if (!isEnrolled)
        {
            TempData["Error"] = "Bạn chưa đăng ký khóa học này.";
            return RedirectToAction("Details", "Courses", new { area = "Student", id = courseId });
        }

        var course = await _courseService.GetCourseByIdAsync(courseId);
        if (course == null)
            return NotFound();

        var lessons = await _lessonService.GetLessonsByCourseAsync(courseId);
        var completedLessons = await _progressService.GetCompletedLessonsCountAsync(userId.Value, courseId);
        var progressPercentage = await _progressService.CalculateProgressPercentageAsync(userId.Value, courseId);

        // Get completion status for each lesson
        var lessonViewModels = new List<LearningLessonViewModel>();
        foreach (var lesson in lessons)
        {
            var isCompleted = await _progressService.IsLessonCompletedAsync(userId.Value, lesson.LessonId);
            lessonViewModels.Add(new LearningLessonViewModel
            {
                LessonId = lesson.LessonId,
                Title = lesson.Title,
                Description = lesson.Description,
                VideoUrl = lesson.ContentUrl,
                OrderIndex = lesson.OrderIndex,
                IsCompleted = isCompleted
            });
        }

        var viewModel = new LearningLessonsViewModel
        {
            CourseId = course.CourseId,
            CourseTitle = course.Title,
            CourseCategoryId = course.CategoryId,
            CourseCategoryName = course.CategoryEntity?.Name ?? "Chưa phân loại",
            TotalLessons = lessons.Count(),
            CompletedLessons = completedLessons,
            ProgressPercentage = progressPercentage,
            Lessons = lessonViewModels
        };

        return View(viewModel);
    }

    // GET: Student/Learning/Content/{lessonId}
    public async Task<IActionResult> Content(int lessonId)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return RedirectToAction("Login", "Account", new { area = "" });

        var lesson = await _lessonService.GetLessonByIdAsync(lessonId);
        if (lesson == null)
            return NotFound();

        // Check enrollment
        var isEnrolled = await _enrollmentService.IsEnrolledAsync(userId.Value, lesson.CourseId);
        if (!isEnrolled)
        {
            TempData["Error"] = "Bạn chưa đăng ký khóa học này.";
            return RedirectToAction("Details", "Courses", new { area = "Student", id = lesson.CourseId });
        }

        var course = await _courseService.GetCourseByIdAsync(lesson.CourseId);
        var allLessons = (await _lessonService.GetLessonsByCourseAsync(lesson.CourseId)).ToList();
        var isCompleted = await _progressService.IsLessonCompletedAsync(userId.Value, lessonId);
        var progressPercentage = await _progressService.CalculateProgressPercentageAsync(userId.Value, lesson.CourseId);
        var completedCount = await _progressService.GetCompletedLessonsCountAsync(userId.Value, lesson.CourseId);

        // Find previous and next lessons
        var currentIndex = allLessons.FindIndex(l => l.LessonId == lessonId);
        var previousLesson = currentIndex > 0 ? allLessons[currentIndex - 1] : null;
        var nextLesson = currentIndex < allLessons.Count - 1 ? allLessons[currentIndex + 1] : null;

        // Build sidebar lessons with completion status
        var sidebarLessons = new List<LearningLessonViewModel>();
        foreach (var l in allLessons)
        {
            var completed = await _progressService.IsLessonCompletedAsync(userId.Value, l.LessonId);
            sidebarLessons.Add(new LearningLessonViewModel
            {
                LessonId = l.LessonId,
                Title = l.Title,
                Description = l.Description,
                VideoUrl = l.ContentUrl,
                OrderIndex = l.OrderIndex,
                IsCompleted = completed
            });
        }

        var viewModel = new LearningContentViewModel
        {
            LessonId = lesson.LessonId,
            LessonTitle = lesson.Title,
            LessonDescription = lesson.Description,
            VideoUrl = lesson.ContentUrl,
            OrderIndex = lesson.OrderIndex,
            IsCompleted = isCompleted,
            CourseId = lesson.CourseId,
            CourseTitle = course?.Title ?? "",
            CourseCategoryId = course?.CategoryId,
            CourseCategoryName = course?.CategoryEntity?.Name ?? "Chưa phân loại",
            InstructorName = course?.Instructor?.FullName ?? "Unknown",
            TotalLessons = allLessons.Count,
            CompletedLessons = completedCount,
            ProgressPercentage = progressPercentage,
            PreviousLessonId = previousLesson?.LessonId,
            NextLessonId = nextLesson?.LessonId,
            AllLessons = sidebarLessons
        };

        return View(viewModel);
    }

    // POST: Student/Learning/MarkComplete
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkComplete(int lessonId)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return RedirectToAction("Login", "Account", new { area = "" });

        var lesson = await _lessonService.GetLessonByIdAsync(lessonId);
        if (lesson == null)
            return NotFound();

        // Check enrollment
        var isEnrolled = await _enrollmentService.IsEnrolledAsync(userId.Value, lesson.CourseId);
        if (!isEnrolled)
        {
            TempData["Error"] = "Bạn chưa đăng ký khóa học này.";
            return RedirectToAction("Details", "Courses", new { area = "Student", id = lesson.CourseId });
        }

        await _progressService.MarkLessonCompleteAsync(userId.Value, lessonId);
        TempData["Success"] = "Đã hoàn thành bài học!";

        // Find next lesson
        var allLessons = (await _lessonService.GetLessonsByCourseAsync(lesson.CourseId)).ToList();
        var currentIndex = allLessons.FindIndex(l => l.LessonId == lessonId);
        var nextLesson = currentIndex < allLessons.Count - 1 ? allLessons[currentIndex + 1] : null;

        if (nextLesson != null)
        {
            return RedirectToAction(nameof(Content), new { lessonId = nextLesson.LessonId });
        }

        return RedirectToAction(nameof(Lessons), new { courseId = lesson.CourseId });
    }
}
