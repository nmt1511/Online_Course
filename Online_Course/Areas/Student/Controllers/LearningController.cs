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

        // Kiểm tra đăng ký và lấy thông tin chi tiết | Check enrollment and get details
        var enrollments = await _enrollmentService.GetEnrollmentsByStudentAsync(userId.Value);
        var enrollment = enrollments.FirstOrDefault(e => e.CourseId == courseId);

        if (enrollment == null)
        {
            TempData["Error"] = "Bạn chưa đăng ký khóa học này.";
            return RedirectToAction("Details", "Courses", new { area = "Student", id = courseId });
        }

        var course = await _courseService.GetCourseByIdAsync(courseId);
        if (course == null)
            return NotFound();

        var lessons = await _lessonService.GetLessonsByCourseAsync(courseId);
        var completedLessonsCount = await _progressService.GetCompletedLessonsCountAsync(userId.Value, courseId);
        var progressPercentage = await _progressService.CalculateProgressPercentageAsync(userId.Value, courseId);

        // Xử lý danh sách bài học với logic khóa tuần tự | Process lessons with sequential locking logic
        var lessonViewModels = new List<LearningLessonViewModel>();
        bool previousLessonCompleted = true; // Bài đầu tiên luôn được mở | First lesson is always open

        foreach (var lesson in lessons.OrderBy(l => l.OrderIndex))
        {
            var isCompleted = await _progressService.IsLessonCompletedAsync(userId.Value, lesson.LessonId);
            
            lessonViewModels.Add(new LearningLessonViewModel
            {
                LessonId = lesson.LessonId,
                Title = lesson.Title,
                Description = lesson.Description,
                VideoUrl = lesson.ContentUrl,
                OrderIndex = lesson.OrderIndex,
                IsCompleted = isCompleted,
                IsLocked = !previousLessonCompleted // Khóa nếu bài trước chưa xong | Lock if previous not completed
            });

            previousLessonCompleted = isCompleted;
        }

        var viewModel = new LearningLessonsViewModel
        {
            CourseId = course.CourseId,
            CourseTitle = course.Title,
            CourseCategoryId = course.CategoryId,
            CourseCategoryName = course.CategoryEntity?.Name ?? "Chưa phân loại",
            TotalLessons = lessons.Count(),
            CompletedLessons = completedLessonsCount,
            ProgressPercentage = progressPercentage,
            CourseStatus = enrollment.LearningStatus, // Gán trạng thái khóa học | Assign course status
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

        // Kiểm tra đăng ký | Check enrollment
        var isEnrolled = await _enrollmentService.IsEnrolledAsync(userId.Value, lesson.CourseId);
        if (!isEnrolled)
        {
            TempData["Error"] = "Bạn chưa đăng ký khóa học này.";
            return RedirectToAction("Details", "Courses", new { area = "Student", id = lesson.CourseId });
        }

        // Kiểm tra logic khóa bài học (Bảo mật) | Check lesson locking logic (Security)
        var allLessons = (await _lessonService.GetLessonsByCourseAsync(lesson.CourseId)).OrderBy(l => l.OrderIndex).ToList();
        var currentIndex = allLessons.FindIndex(l => l.LessonId == lessonId);
        
        if (currentIndex > 0)
        {
            var previousLessonId = allLessons[currentIndex - 1].LessonId;
            var isPreviousCompleted = await _progressService.IsLessonCompletedAsync(userId.Value, previousLessonId);
            if (!isPreviousCompleted)
            {
                TempData["Error"] = "Bạn cần hoàn thành bài học trước đó để tiếp tục."; // Thông báo rõ ràng theo yêu cầu | Clear message as requested
                return RedirectToAction(nameof(Lessons), new { courseId = lesson.CourseId });
            }
        }

        var course = await _courseService.GetCourseByIdAsync(lesson.CourseId);
        var isCompleted = await _progressService.IsLessonCompletedAsync(userId.Value, lessonId);
        var progressPercentage = await _progressService.CalculateProgressPercentageAsync(userId.Value, lesson.CourseId);
        var completedCount = await _progressService.GetCompletedLessonsCountAsync(userId.Value, lesson.CourseId);

        // Tìm bài học trước và sau | Find previous and next lessons
        var previousLesson = currentIndex > 0 ? allLessons[currentIndex - 1] : null;
        var nextLesson = currentIndex < allLessons.Count - 1 ? allLessons[currentIndex + 1] : null;

        // Xây dựng danh sách sidebar với trạng thái khóa | Build sidebar list with locking status
        var sidebarLessons = new List<LearningLessonViewModel>();
        bool prevComp = true; 
        foreach (var l in allLessons)
        {
            var comp = await _progressService.IsLessonCompletedAsync(userId.Value, l.LessonId);
            sidebarLessons.Add(new LearningLessonViewModel
            {
                LessonId = l.LessonId,
                Title = l.Title,
                Description = l.Description,
                VideoUrl = l.ContentUrl,
                OrderIndex = l.OrderIndex,
                IsCompleted = comp,
                IsLocked = !prevComp
            });
            prevComp = comp;
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
