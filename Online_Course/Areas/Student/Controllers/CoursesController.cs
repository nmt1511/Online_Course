using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Online_Course.Helper;
using Online_Course.Models;
using Online_Course.Services.CategoryService;
using Online_Course.Services.CourseService;
using Online_Course.Services.EnrollmentService;
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
    // Xử lý yêu cầu ghi danh của học viên vào một khóa học công khai cụ thể
    public async Task<IActionResult> Enroll(int courseId)
    {
        var userIdClaim = User.FindFirst("UserId")?.Value;
        if (!int.TryParse(userIdClaim, out int userId))
        {
            return RedirectToAction("Login", "Account", new { area = "" });
        }

        // Xác thực sự tồn tại và trạng thái hiển thị công khai của khóa học trước khi tiến hành ghi danh
        var course = await _courseService.GetCourseByIdAsync(courseId);
        if (course == null || course.CourseStatus != CourseStatus.Public)
        {
            TempData["Error"] = "Khóa học không tồn tại hoặc chưa được xuất bản.";
            return RedirectToAction(nameof(Index));
        }

        // Đảm bảo không ghi danh trùng lặp cho các khóa học mà học viên đã tham gia
        var isEnrolled = await _enrollmentService.IsEnrolledAsync(userId, courseId);
        if (isEnrolled)
        {
            TempData["Info"] = "Đã thực hiện đăng ký khóa học này từ trước.";
            return RedirectToAction(nameof(Details), new { id = courseId });
        }

        // Nếu khóa học là 'Thời gian cố định', hãy kiểm tra ngày đăng ký.
        if (course.CourseType == CourseType.Fixed_Time)
        {
            var today = DateTimeHelper.GetVietnamTimeNow().Date;

            // Kiểm tra xem ngày bắt đầu hoặc ngày kết thúc đăng ký có bị thiếu không.
            if (!course.RegistrationStartDate.HasValue || !course.RegistrationEndDate.HasValue)
            {
                TempData["Error"] = "Khóa học này chưa được thiết lập thời gian đăng ký.";
                return RedirectToAction(nameof(Details), new { id = courseId });
            }

            // Hãy kiểm tra xem ngày bắt đầu đăng ký đã đến chưa.
            if (today < course.RegistrationStartDate.Value.Date)
            {
                TempData["Error"] = $"Khóa học chưa mở đăng ký. Thời gian đăng ký từ: {course.RegistrationStartDate.Value:dd/MM/yyyy}.";
                return RedirectToAction(nameof(Details), new { id = courseId });
            }

            // Hãy kiểm tra xem ngày kết thúc đăng ký đã hết chưa.
            if (today > course.RegistrationEndDate.Value.Date)
            {
                TempData["Error"] = $"Thời gian đăng ký khóa học này đã kết thúc vào ngày {course.RegistrationEndDate.Value:dd/MM/yyyy}.";
                return RedirectToAction(nameof(Details), new { id = courseId });
            }
        }

        // tạo bản ghi đăng ký khóa học
        await _enrollmentService.EnrollAsync(userId, courseId);
        TempData["Success"] = "Đăng ký khóa học thành công!";

        return RedirectToAction("Lessons", "Learning", new { area = "Student", courseId = courseId });
    }

    [AllowAnonymous]
    public async Task<IActionResult> Index(int? categoryId, CourseType? type)
    {
        var courses = !categoryId.HasValue
            ? await _courseService.GetAllCoursesPublicAsync()
            : await _courseService.GetCoursesByCategoryAsync(categoryId.Value);

        // Lọc danh sách theo loại khóa học nếu được yêu cầu.
        if (type.HasValue)
        {
            courses = courses.Where(c => c.CourseType == type.Value);
        }


        var categories = await _categoryService.GetAllCategoriesAsync();

        var viewModel = new StudentCourseIndexViewModel
        {
            Courses = courses.Select(c => new StudentCourseListViewModel
            {
                CourseId = c.CourseId,
                Title = c.Title,
                Description = c.Description,
                CategoryId = c.CategoryId,
                CategoryName = c.CategoryEntity?.Name ?? "Chưa phân loại",
                ThumbnailUrl = c.ThumbnailUrl,
                InstructorName = c.Instructor?.FullName ?? "Unknown",
                EnrollmentCount = c.Enrollments?.Count ?? 0,
                LessonCount = c.Lessons?.Count ?? 0,
                CourseType = c.CourseType
            }).ToList(),
            Categories = categories.Where(c => c.IsActive).ToList(),
            SelectedCategoryId = categoryId,
            SelectedCourseType = type
        };

        return View(viewModel);
    }

    [AllowAnonymous]
    // Hiển thị chi tiết thông tin và danh mục bài học của khóa học (phục vụ mục đích xem trước)
    public async Task<IActionResult> Details(int id)
    {
        var course = await _courseService.GetCourseByIdAsync(id);

        if (course == null || course.CourseStatus != CourseStatus.Public)
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
            CategoryId = course.CategoryId,
            CategoryName = course.CategoryEntity?.Name ?? "Chưa phân loại",
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
            IsEnrolled = isEnrolled,
            CourseType = course.CourseType,
            StartDate = course.StartDate,
            EndDate = course.EndDate,
            RegistrationStartDate = course.RegistrationStartDate,
            RegistrationEndDate = course.RegistrationEndDate
        };

        return View(viewModel);
    }

    [HttpPost]
    [Authorize(Roles = "Student")]
    [ValidateAntiForgeryToken]
    // Xử lý yêu cầu hủy đăng ký khóa học của học viên
    public async Task<IActionResult> Unenroll(int courseId)
    {
        var userIdClaim = User.FindFirst("UserId")?.Value;
        if (!int.TryParse(userIdClaim, out int userId))
        {
            return RedirectToAction("Login", "Account", new { area = "" });
        }

        // Kiểm tra xác thực trạng thái đăng ký trước khi thực hiện hủy
        var isEnrolled = await _enrollmentService.IsEnrolledAsync(userId, courseId);
        if (!isEnrolled)
        {
            TempData["Error"] = "Bạn chưa đăng ký khóa học này.";
            return RedirectToAction("Index", "Progress", new { area = "Student" });
        }

        // Thực hiện logic hủy ghi danh học viên khỏi khóa học
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