using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Online_Course.Services;
using Online_Course.ViewModels;

namespace Online_Course.Areas.Student.Controllers;

[Area("Student")]
[Authorize(Roles = "Student")]
public class ProgressController : Controller
{
    private readonly IEnrollmentService _enrollmentService;
    private readonly IProgressService _progressService;
    private readonly ILessonService _lessonService;

    public ProgressController(
        IEnrollmentService enrollmentService,
        IProgressService progressService,
        ILessonService lessonService)
    {
        _enrollmentService = enrollmentService;
        _progressService = progressService;
        _lessonService = lessonService;
    }

    private int? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("UserId")?.Value;
        if (int.TryParse(userIdClaim, out int userId))
            return userId;
        return null;
    }

    // GET: Student/Progress
    public async Task<IActionResult> Index()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return RedirectToAction("Login", "Account", new { area = "" });

        var enrollments = await _enrollmentService.GetEnrollmentsByStudentAsync(userId.Value);
        
        var courseProgressList = new List<StudentCourseProgressViewModel>();

        //Tổng khóa học hoàn thành
        int totalCompletedCourses = 0;

        //Tổng % hoàn thành của sinh viên
        double overallProgressSum = 0;

        // Duyệt qua danh sách đăng ký
        foreach (var enrollment in enrollments)
        {
            // Lấy tổng số bài học và số bài học đã hoàn thành
            var totalLessons = enrollment.Course?.Lessons?.Count ?? 0;
            var completedLessons = await _progressService.GetCompletedLessonsCountAsync(userId.Value, enrollment.CourseId);
            
            // Tính toán phần trăm tiến độ
            var progressPercentage = await _progressService.CalculateProgressPercentageAsync(userId.Value, enrollment.CourseId);
            
            // Xác định trạng thái hiển thị dựa trên LearningStatus từ model
            string status = "";
            switch (enrollment.LearningStatus)
            {
                case Online_Course.Models.LearningStatus.NOT_STARTED:
                    status = "Chưa học";
                    break;
                case Online_Course.Models.LearningStatus.IN_PROGRESS:
                    status = "Đang học";
                    break;
                case Online_Course.Models.LearningStatus.COMPLETED:
                    status = "Hoàn thành";
                    totalCompletedCourses++; // Tăng số lượng khóa học đã hoàn thành
                    break;
                default:
                    status = "Chưa học";
                    break;
            }

            overallProgressSum += progressPercentage;

            // Lấy bài học đang học trong khóa học
            var lessons = await _lessonService.GetLessonsByCourseAsync(enrollment.CourseId);
            string currentLessonTitle = "";
            //Lấy từng bài học trong khóa học theo thứ tự để kiểm tra % hoàn thành
            foreach (var lesson in lessons.OrderBy(l => l.OrderIndex))
            {
                //Kiểm tra bài học đã hoàn thành chưa? chưa thì dừng
                var isCompleted = await _progressService.IsLessonCompletedAsync(userId.Value, lesson.LessonId);
                if (!isCompleted)
                {
                    currentLessonTitle = $"Bài {lesson.OrderIndex}: {lesson.Title}";
                    break;
                }
            }

            // Thêm thông tin vào danh sách hiển thị
            courseProgressList.Add(new StudentCourseProgressViewModel
            {
                CourseId = enrollment.CourseId,
                Title = enrollment.Course?.Title ?? "",
                CategoryId = enrollment.Course?.CategoryId,
                CategoryName = enrollment.Course?.CategoryEntity?.Name ?? "Chưa phân loại",
                ThumbnailUrl = enrollment.Course?.ThumbnailUrl ?? "",
                TotalLessons = totalLessons,
                CompletedLessons = completedLessons,
                ProgressPercentage = progressPercentage,
                Status = status,
                LearningStatus = enrollment.LearningStatus, // Gán trạng thái học tập
                IsMandatory = enrollment.IsMandatory, // Gán thuộc tính bắt buộc
                EnrolledAt = enrollment.EnrolledAt,
                CurrentLessonTitle = currentLessonTitle
            });
        }

        var totalCourses = courseProgressList.Count;
        var overallProgress = totalCourses > 0 ? overallProgressSum / totalCourses : 0;

        var viewModel = new StudentProgressIndexViewModel
        {
            TotalCourses = totalCourses,
            CompletedCourses = totalCompletedCourses,
            OverallProgress = Math.Round(overallProgress, 1),
            Courses = courseProgressList
        };

        return View(viewModel);
    }
}
