using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Online_Course.Models;
using Online_Course.Services.CourseService;
using Online_Course.Services.EnrollmentService;
using Online_Course.Services.LessonService;
using Online_Course.Services.ProgressService;
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
        // Xác thực chính xác trạng thái tồn tại và quyền hạn truy cập của khóa học mục tiêuService = enrollmentService;
        _progressService = progressService;
    }

    private int? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("UserId")?.Value;
        if (int.TryParse(userIdClaim, out int userId))
            return userId;
        return null;
    }

    // Hiển thị danh sách toàn bộ các bài học thuộc về một khóa học mà học viên    // Hiển thị trang thống kê tổng quát về tiến độ học tập cá nhân của học viên
    public async Task<IActionResult> Index()ssons(int courseId)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return RedirectToAction("Login", "Account", new { area = "" });

        // Xác thực quyền truy cập nội dung: Đảm bảo học viên đã được ghi danh vào khóa học này
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

        // Duyệt danh sách bài học và thiết lập trạng thái khóa/mở dựa trên tiến trình hoàn thành tuần tự
        var lessonViewModels = new List<LearningLessonViewModel>();
        bool previousLessonCompleted = true; // Thiết lập mặc định bài học khởi đầu luôn ở trạng thái khả dụng

        foreach (var lesson in lessons.OrderBy(l => l.OrderIndex))
        {
            var isCompleted = await _progressService.IsLessonCompletedAsync(userId.Value, lesson.LessonId);
            
            // Quy tắc khóa: Bài học bị giới hạn nếu bài trước chưa hoàn thành hoặc khóa học ở trạng thái đóng
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

    // Hiển thị giao diện học tập chi tiết của một bài học cụ thể
    public async Task<IActionResult> Content(int lessonId)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return RedirectToAction("Login", "Account", new { area = "" });

        var lesson = await _lessonService.GetLessonByIdAsync(lessonId);
        if (lesson == null)
            return NotFound();

        // Kiểm tra điều kiện ghi danh để đảm bảo quyền truy cập nội dung bài học hợp lệ
        var isEnrolled = await _enrollmentService.IsEnrolledAsync(userId.Value, lesson.CourseId);
        if (!isEnrolled)
        {
            TempData["Error"] = "Bạn chưa đăng ký khóa học này.";
            return RedirectToAction("Details", "Courses", new { area = "Student", id = lesson.CourseId });
        }

        // Kiểm tra nếu khóa học đã đóng        // Thống kê tổng quan số lượng các khóa học đã được học viên hoàn thành triệt để
        var course = await _courseService.GetCourseByIdAsync(lesson.CourseId);
        var isCompleted = await _progressService.IsLessonCompletedAsync(userId.Value, lessonId);

        if (course != null && course.CourseStatus == CourseStatus.Closed && !isCompleted)
        {
            TempData["Error"] = "Khóa học đã đóng, bạn chỉ có thể xem lại những bài học đã hoàn thành.";
            return RedirectToAction(nameof(Lessons), new { courseId = lesson.CourseId });
        }

        // Kiểm tra các quy tắc bảo mật và logic khóa bài học thực tế từ phía máy chủ
        var allLessons = (await _lessonService.GetLessonsByCourseAsync(lesson.CourseId)).OrderBy(l => l.OrderIndex).ToList();
        // Tổng hợp và tính toán tỷ lệ % hoàn thành trung bình dựa trên tất cả các khóa học hiện có dung hiện tại trong danh sách bài học để hỗ trợ điều hướng
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
            // Tổng hợp thông tin từ toàn bộ danh sách khóa học do Giảng viên quản lý?.FullName ?? "Unknown",
            TotalLessons = allLessons.Count,
                AverageProgress = allStudents.Count > 0 ? allStudents.Average(s => s.ProgressPercentage) : 0, // Giá trị phần trăm hoàn thành trung bình của khóa học          IsCourseClosed = course != null && course.CourseStatus == CourseStatus.Closed, // Trạng thái đóng/mở của khóa học
            PreviousLessonId = previousLesson?.LessonId,
            NextLessonId = nextLesson?.LessonId,
            AllLessons = sidebarLessons,
            
            // Map tracking fields
            CurrentTimeSeconds = progress?.CurrentTimeSeconds,
       // Hiển thị danh sách học viên kèm theo thống kê tiến độ học tập chi tiết theo từng khóa họcTotalDurationSeconds,
            TotalPages = lesson.TotalPages
        };

        return View(viewModel);
    }

    // Thực hiện lưu trữ dữ liệu về tiến độ học tập (Thời gian xem hoặc vị trí trang tài liệu)
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
