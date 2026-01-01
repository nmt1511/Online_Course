using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Online_Course.Models;
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
            
            // Logic khóa bài học: Bài học bị khóa nếu:
            // 1. Bài học trước chưa hoàn thành (logic cũ)
            // 2. HOẶC Khóa học đã đóng và bài này chưa hoàn thành (logic mới)
            bool isLocked = !previousLessonCompleted;
            if (course.CourseStatus == CourseStatus.Closed && !isCompleted)
            {
                isLocked = true;
            }

            lessonViewModels.Add(new LearningLessonViewModel
            {
                LessonId = lesson.LessonId,
                Title = lesson.Title,
                Description = lesson.Description,
                ContentUrl = lesson.ContentUrl,
                LessonType = lesson.LessonType,
                OrderIndex = lesson.OrderIndex,
                IsCompleted = isCompleted,
                IsLocked = isLocked 
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
            CourseStatus = enrollment.LearningStatus, // Trạng thái học tập của học viên
            IsCourseClosed = course.CourseStatus == CourseStatus.Closed, // Trạng thái đóng/mở của khóa học
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

        // Kiểm tra nếu khóa học đã đóng và bài học này chưa hoàn thành
        var course = await _courseService.GetCourseByIdAsync(lesson.CourseId);
        var isCompleted = await _progressService.IsLessonCompletedAsync(userId.Value, lessonId);

        if (course != null && course.CourseStatus == CourseStatus.Closed && !isCompleted)
        {
            TempData["Error"] = "Khóa học đã đóng, bạn chỉ có thể xem lại những bài học đã hoàn thành.";
            return RedirectToAction(nameof(Lessons), new { courseId = lesson.CourseId });
        }

        // Kiểm tra logic khóa bài học (Bảo mật) | Check lesson locking logic (Security)
        var allLessons = (await _lessonService.GetLessonsByCourseAsync(lesson.CourseId)).OrderBy(l => l.OrderIndex).ToList();
        //index bài học vừa chọn
        var currentIndex = allLessons.FindIndex(l => l.LessonId == lessonId);
        
        if (currentIndex > 0)
        {
            //Kiểm tra bài học trước đã hoàn thành chưa
            var previousLessonId = allLessons[currentIndex - 1].LessonId;
            var isPreviousCompleted = await _progressService.IsLessonCompletedAsync(userId.Value, previousLessonId);
            if (!isPreviousCompleted)
            {
                TempData["Error"] = "Bạn cần hoàn thành bài học trước đó để tiếp tục."; // Thông báo rõ ràng theo yêu cầu | Clear message as requested
                return RedirectToAction(nameof(Lessons), new { courseId = lesson.CourseId });
            }
        }

        var progressPercentage = await _progressService.CalculateProgressPercentageAsync(userId.Value, lesson.CourseId);
        var completedCount = await _progressService.GetCompletedLessonsCountAsync(userId.Value, lesson.CourseId);

        // Tìm bài học trước và sau
        var previousLesson = currentIndex > 0 ? allLessons[currentIndex - 1] : null;
        var nextLesson = currentIndex < allLessons.Count - 1 ? allLessons[currentIndex + 1] : null;

        // Xây dựng danh sách sidebar với trạng thái khóa 
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
                ContentUrl = l.ContentUrl,
                LessonType = l.LessonType,
                OrderIndex = l.OrderIndex,
                IsCompleted = comp,
                IsLocked = !prevComp
            });
            prevComp = comp;
        }

        // Lấy tiến độ hiện tại 1m
        var progress = (await _progressService.GetProgressByStudentAndCourseAsync(userId.Value, lesson.CourseId))
                        .FirstOrDefault(p => p.LessonId == lessonId);

        var viewModel = new LearningContentViewModel
        {
            LessonId = lesson.LessonId,
            LessonTitle = lesson.Title,
            LessonDescription = lesson.Description,
            ContentUrl = lesson.ContentUrl,
            LessonType = lesson.LessonType,
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
            IsCourseClosed = course != null && course.CourseStatus == CourseStatus.Closed, // Trạng thái đóng/mở của khóa học
            PreviousLessonId = previousLesson?.LessonId,
            NextLessonId = nextLesson?.LessonId,
            AllLessons = sidebarLessons,
            
            // Map tracking fields
            CurrentTimeSeconds = progress?.CurrentTimeSeconds,
            CurrentPage = progress?.CurrentPage,
            TotalDurationSeconds = lesson.TotalDurationSeconds,
            TotalPages = lesson.TotalPages
        };

        return View(viewModel);
    }

    // POST: Student/Learning/SaveProgress
    [HttpPost]
    public async Task<IActionResult> SaveProgress(int lessonId, int? currentTime, int? currentPage, bool isCompleted)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        try
        {
            await _progressService.UpdateProgressAsync(userId.Value, lessonId, currentTime, currentPage, isCompleted);
            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }
}
