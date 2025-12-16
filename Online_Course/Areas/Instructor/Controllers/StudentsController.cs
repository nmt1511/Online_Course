using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Online_Course.Services;
using Online_Course.ViewModels;
using System.Security.Claims;

namespace Online_Course.Areas.Instructor.Controllers;

[Area("Instructor")]
[Authorize(Policy = "InstructorOnly")]
public class StudentsController : Controller
{
    private readonly ICourseService _courseService;
    private readonly IEnrollmentService _enrollmentService;
    private readonly IProgressService _progressService;

    public StudentsController(
        ICourseService courseService,
        IEnrollmentService enrollmentService,
        IProgressService progressService)
    {
        _courseService = courseService;
        _enrollmentService = enrollmentService;
        _progressService = progressService;
    }

    public async Task<IActionResult> Index(int? courseId = null)
    {
        var instructorId = int.Parse(User.FindFirstValue("UserId") ?? "0");
        
        if (!courseId.HasValue)
        {
            // Show all students across all instructor's courses
            var courses = await _courseService.GetCoursesByInstructorAsync(instructorId);
            var allStudents = new List<StudentProgressViewModel>();
            
            foreach (var course in courses)
            {
                var enrollments = await _enrollmentService.GetEnrollmentsByCourseAsync(course.CourseId);
                var totalLessons = course.Lessons?.Count ?? 0;
                
                foreach (var enrollment in enrollments)
                {
                    var progressPercentage = await _progressService.CalculateProgressPercentageAsync(
                        enrollment.StudentId, course.CourseId);
                    var completedLessons = await _progressService.GetCompletedLessonsCountAsync(
                        enrollment.StudentId, course.CourseId);

                    allStudents.Add(new StudentProgressViewModel
                    {
                        StudentId = enrollment.StudentId,
                        FullName = enrollment.Student.FullName,
                        Email = enrollment.Student.Email,
                        EnrolledAt = enrollment.EnrolledAt,
                        ProgressPercentage = progressPercentage,
                        CompletedLessons = completedLessons,
                        TotalLessons = totalLessons,
                        Status = GetStatus(progressPercentage),
                        CourseName = course.Title,
                        CourseId = course.CourseId
                    });
                }
            }
            
            var viewModel = new CourseStudentsViewModel
            {
                CourseId = 0,
                CourseTitle = "Tất cả khóa học",
                TotalStudents = allStudents.Select(s => s.StudentId).Distinct().Count(),
                AverageProgress = allStudents.Count > 0 ? allStudents.Average(s => s.ProgressPercentage) : 0,
                CompletedCount = allStudents.Count(s => s.ProgressPercentage >= 100),
                Students = allStudents.OrderByDescending(s => s.EnrolledAt).ToList()
            };

            return View(viewModel);
        }
        
        var selectedCourse = await _courseService.GetCourseByIdAsync(courseId.Value);
        if (selectedCourse == null || selectedCourse.CreatedBy != instructorId)
        {
            return NotFound();
        }

        var courseEnrollments = await _enrollmentService.GetEnrollmentsByCourseAsync(courseId.Value);
        var courseTotalLessons = selectedCourse.Lessons?.Count ?? 0;

        var students = new List<StudentProgressViewModel>();
        double totalProgress = 0;
        int completedCount = 0;

        foreach (var enrollment in courseEnrollments)
        {
            var progressPercentage = await _progressService.CalculateProgressPercentageAsync(
                enrollment.StudentId, courseId.Value);
            var completedLessons = await _progressService.GetCompletedLessonsCountAsync(
                enrollment.StudentId, courseId.Value);

            var status = GetStatus(progressPercentage);
            if (progressPercentage >= 100)
            {
                completedCount++;
            }

            students.Add(new StudentProgressViewModel
            {
                StudentId = enrollment.StudentId,
                FullName = enrollment.Student.FullName,
                Email = enrollment.Student.Email,
                EnrolledAt = enrollment.EnrolledAt,
                ProgressPercentage = progressPercentage,
                CompletedLessons = completedLessons,
                TotalLessons = courseTotalLessons,
                Status = status
            });

            totalProgress += progressPercentage;
        }

        var result = new CourseStudentsViewModel
        {
            CourseId = courseId.Value,
            CourseTitle = selectedCourse.Title,
            TotalStudents = students.Count,
            AverageProgress = students.Count > 0 ? totalProgress / students.Count : 0,
            CompletedCount = completedCount,
            Students = students.OrderByDescending(s => s.EnrolledAt).ToList()
        };

        return View(result);
    }

    private static string GetStatus(double progressPercentage)
    {
        return progressPercentage switch
        {
            >= 100 => "Đã hoàn thành",
            >= 90 => "Sắp xong",
            >= 1 => "Đang học",
            _ => "Chưa bắt đầu"
        };
    }
}
