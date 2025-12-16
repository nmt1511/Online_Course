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
        int totalCompletedCourses = 0;
        double overallProgressSum = 0;

        foreach (var enrollment in enrollments)
        {
            var totalLessons = enrollment.Course?.Lessons?.Count ?? 0;
            var completedLessons = await _progressService.GetCompletedLessonsCountAsync(userId.Value, enrollment.CourseId);
            var progressPercentage = await _progressService.CalculateProgressPercentageAsync(userId.Value, enrollment.CourseId);
            
            var status = progressPercentage >= 100 ? "Hoàn thành" : "Đang học";
            if (progressPercentage >= 100)
                totalCompletedCourses++;

            overallProgressSum += progressPercentage;

            // Get the current lesson (first incomplete lesson)
            var lessons = await _lessonService.GetLessonsByCourseAsync(enrollment.CourseId);
            string currentLessonTitle = "";
            foreach (var lesson in lessons.OrderBy(l => l.OrderIndex))
            {
                var isCompleted = await _progressService.IsLessonCompletedAsync(userId.Value, lesson.LessonId);
                if (!isCompleted)
                {
                    currentLessonTitle = $"Bài {lesson.OrderIndex}: {lesson.Title}";
                    break;
                }
            }

            courseProgressList.Add(new StudentCourseProgressViewModel
            {
                CourseId = enrollment.CourseId,
                Title = enrollment.Course?.Title ?? "",
                Category = enrollment.Course?.Category ?? "",
                ThumbnailUrl = enrollment.Course?.ThumbnailUrl ?? "",
                TotalLessons = totalLessons,
                CompletedLessons = completedLessons,
                ProgressPercentage = progressPercentage,
                Status = status,
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
