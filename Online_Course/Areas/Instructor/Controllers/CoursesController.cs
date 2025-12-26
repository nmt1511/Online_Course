using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Online_Course.Helper;
using Online_Course.Models;
using Online_Course.Services;
using Online_Course.ViewModels;
using System.Security.Claims;

namespace Online_Course.Areas.Instructor.Controllers;

[Area("Instructor")]
[Authorize(Policy = "InstructorOnly")]
public class CoursesController : Controller
{
    private readonly ICourseService _courseService;
    private readonly IEnrollmentService _enrollmentService;
    private readonly ICategoryService _categoryService;
    private readonly IProgressService _progressService;
    private readonly IUserService _userService;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public CoursesController(
        ICourseService courseService,
        IEnrollmentService enrollmentService,
        ICategoryService categoryService,
        IProgressService progressService,
        IUserService userService,
        IWebHostEnvironment webHostEnvironment)
    {
        _courseService = courseService;
        _enrollmentService = enrollmentService;
        _categoryService = categoryService;
        _progressService = progressService;
        _userService = userService;
        _webHostEnvironment = webHostEnvironment;
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        return userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;
    }

    public async Task<IActionResult> Index()
    {
        var instructorId = GetCurrentUserId();
        var courses = await _courseService.GetCoursesByInstructorAsync(instructorId);

        var viewModel = courses.Select(c => new InstructorCourseListViewModel
        {
            CourseId = c.CourseId,
            Title = c.Title,
            Description = c.Description,
            CategoryId = c.CategoryId,
            CategoryName = c.CategoryEntity?.Name ?? "Chưa phân loại",
            ThumbnailUrl = c.ThumbnailUrl,
            Status = c.CourseStatus,
            Type = c.CourseType,
            EnrollmentCount = c.Enrollments?.Count ?? 0,
            LessonCount = c.Lessons?.Count ?? 0,
            AverageRating = 4.5 // Placeholder - would come from ratings system
        }).ToList();

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        ViewBag.Categories = await _categoryService.GetAllCategoriesAsync();
        // Lấy danh sách học viên để chọn cho khóa học riêng tư
        var users = await _userService.GetAllUsersAsync();
        ViewBag.Students = users.Where(u => u.UserRoles.Any(ur => ur.Role.Name == "Student")).ToList();
        
        return View(new InstructorCreateCourseViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(InstructorCreateCourseViewModel model, IFormFile? thumbnailFile)
    {
        // Remove ThumbnailUrl from validation - it's optional
        ModelState.Remove("ThumbnailUrl");

        // Validate dates if CourseType is Fixed_Time
        if (model.CourseType == CourseType.Fixed_Time)
        {
            if (!model.RegistrationStartDate.HasValue || !model.RegistrationEndDate.HasValue ||
                !model.StartDate.HasValue || !model.EndDate.HasValue)
            {
                ModelState.AddModelError("", "Vui lòng nhập đầy đủ các ngày khi chọn loại khóa học thời gian cố định.");
            }
            else
            {
                if (model.RegistrationStartDate >= model.RegistrationEndDate)
                {
                    ModelState.AddModelError("RegistrationEndDate", "Ngày kết thúc đăng ký phải sau ngày bắt đầu đăng ký.");
                }
                if (model.RegistrationEndDate >= model.StartDate)
                {
                    ModelState.AddModelError("StartDate", "Ngày bắt đầu học phải sau ngày kết thúc đăng ký.");
                }
                if (model.StartDate >= model.EndDate)
                {
                    ModelState.AddModelError("EndDate", "Ngày kết thúc học phải sau ngày bắt đầu học.");
                }
            }
        }

        if (!ModelState.IsValid)
        {
            ViewBag.Categories = await _categoryService.GetAllCategoriesAsync();
            // Lấy danh sách học viên để chọn cho khóa học riêng tư
            var users_create = await _userService.GetAllUsersAsync();
            ViewBag.Students = users_create.Where(u => u.UserRoles.Any(ur => ur.Role.Name == "Student")).ToList();
            return View(model);
        }

        var instructorId = GetCurrentUserId();
        string thumbnailUrl;

        // Handle file upload first
        if (thumbnailFile != null && thumbnailFile.Length > 0)
        {
            // Check file size (5MB max)
            if (thumbnailFile.Length > 5 * 1024 * 1024)
            {
                ModelState.AddModelError("thumbnailFile", "Kích thước file vượt quá 5MB");
                ViewBag.Categories = await _categoryService.GetAllCategoriesAsync();
                return View(model);
            }
            thumbnailUrl = await SaveImageAsync(thumbnailFile);
        }
        // Then check URL
        else if (!string.IsNullOrWhiteSpace(model.ThumbnailUrl))
        {
            thumbnailUrl = model.ThumbnailUrl;
        }
        // Use default image if no thumbnail provided
        else
        {
            thumbnailUrl = "/images/default-course.png";
        }

        var course = new Course
        {
            Title = model.Title,
            Description = model.Description,
            CategoryId = model.CategoryId,
            ThumbnailUrl = thumbnailUrl,
            CourseStatus = model.Status,
            CourseType = model.CourseType,
            RegistrationStartDate = model.CourseType == CourseType.Fixed_Time ? model.RegistrationStartDate : null,
            RegistrationEndDate = model.CourseType == CourseType.Fixed_Time ? model.RegistrationEndDate : null,
            StartDate = model.CourseType == CourseType.Fixed_Time ? model.StartDate : null,
            EndDate = model.CourseType == CourseType.Fixed_Time ? model.EndDate : null,
            CreatedBy = instructorId
        };

        await _courseService.CreateCourseAsync(course);

        // Xử lý ghi danh học viên nếu là khóa học riêng tư
        if (course.CourseStatus == CourseStatus.Private && model.SelectedStudentIds != null && model.SelectedStudentIds.Any())
        {
            foreach (var studentId in model.SelectedStudentIds)
            {
                var enrollment = new Enrollment
                {
                    CourseId = course.CourseId,
                    StudentId = studentId,
                    IsMandatory = true,
                    LearningStatus = LearningStatus.NOT_STARTED,
                    EnrolledAt = DateTimeHelper.GetVietnamTimeNow(),
                };
                await _enrollmentService.EnrollStudentAsync(enrollment);
            }
        }

        TempData["SuccessMessage"] = "Tạo khóa học mới thành công!";
        return RedirectToAction(nameof(Index));
    }


    private async Task<string> SaveImageAsync(IFormFile file)
    {
        // Create images folder if not exists
        var imagesFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "courses");
        if (!Directory.Exists(imagesFolder))
        {
            Directory.CreateDirectory(imagesFolder);
        }

        // Generate unique filename
        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var filePath = Path.Combine(imagesFolder, fileName);

        // Save file
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // Return relative URL
        return $"/images/courses/{fileName}";
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var instructorId = GetCurrentUserId();
        var course = await _courseService.GetCourseByIdAsync(id);

        if (course == null)
        {
            return NotFound();
        }

        // Verify the course belongs to this instructor
        if (course.CreatedBy != instructorId)
        {
            return Forbid();
        }

        // Get enrollments for this course
        var enrollments = await _enrollmentService.GetEnrollmentsByCourseAsync(id);

        // Build student list with completion percentage
        var students = new List<StudentEnrollmentViewModel>();
        foreach (var enrollment in enrollments)
        {
            var completionPercentage = await _progressService.CalculateProgressPercentageAsync(enrollment.StudentId, id);
            students.Add(new StudentEnrollmentViewModel
            {
                StudentId = enrollment.StudentId,
                FullName = enrollment.Student?.FullName ?? "Unknown",
                Email = enrollment.Student?.Email ?? "",
                EnrolledAt = enrollment.EnrolledAt,
                CompletionPercentage = completionPercentage
            });
        }

        var viewModel = new CourseDetailViewModel
        {
            CourseId = course.CourseId,
            Title = course.Title,
            Description = course.Description,
            CategoryId = course.CategoryId,
            CategoryName = course.CategoryEntity?.Name ?? "Chưa phân loại",
            ThumbnailUrl = course.ThumbnailUrl,
            Status = course.CourseStatus,
            CourseType = course.CourseType,
            RegistrationStartDate = course.RegistrationStartDate,
            RegistrationEndDate = course.RegistrationEndDate,
            StartDate = course.StartDate,
            EndDate = course.EndDate,
            ShowInstructor = false, // Instructor doesn't need to see their own name
            TotalLessons = course.Lessons?.Count ?? 0,
            TotalStudents = enrollments.Count(),
            Lessons = course.Lessons?.OrderBy(l => l.OrderIndex).Select(l => new LessonSummaryViewModel
            {
                LessonId = l.LessonId,
                Title = l.Title,
                Description = l.Description,
                OrderIndex = l.OrderIndex,
                ContentUrl = l.ContentUrl,
                LessonType = l.LessonType,
            }) ?? Enumerable.Empty<LessonSummaryViewModel>(),
            Students = students
        };


        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var instructorId = GetCurrentUserId();
        var course = await _courseService.GetCourseByIdAsync(id);

        if (course == null)
        {
            return NotFound();
        }

        // Verify the course belongs to this instructor
        if (course.CreatedBy != instructorId)
        {
            return Forbid();
        }

        var viewModel = new InstructorEditCourseViewModel
        {
            CourseId = course.CourseId,
            Title = course.Title,
            Description = course.Description,
            CategoryId = course.CategoryId,
            ThumbnailUrl = course.ThumbnailUrl,
            Status = course.CourseStatus,
            CourseType = course.CourseType,
            RegistrationStartDate = course.RegistrationStartDate,
            RegistrationEndDate = course.RegistrationEndDate,
            StartDate = course.StartDate,
            EndDate = course.EndDate,
            InstructorName = course.Instructor?.FullName ?? "",
            EnrollmentCount = course.Enrollments?.Count ?? 0,
            LessonCount = course.Lessons?.Count ?? 0,
            // Lấy danh sách ID học viên đã được ghi danh bắt buộc
            SelectedStudentIds = course.Enrollments?.Where(e => e.IsMandatory).Select(e => e.StudentId).ToList() ?? new List<int>()
        };

        ViewBag.Categories = await _categoryService.GetAllCategoriesAsync();
        // Lấy danh sách học viên để chọn
        var users = await _userService.GetAllUsersAsync();
        ViewBag.Students = users.Where(u => u.UserRoles.Any(ur => ur.Role.Name == "Student")).ToList();
        
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, InstructorEditCourseViewModel model, IFormFile? thumbnailFile)
    {
        if (id != model.CourseId)
        {
            return BadRequest();
        }

        var instructorId = GetCurrentUserId();
        var existingCourse = await _courseService.GetCourseByIdAsync(id);

        if (existingCourse == null)
        {
            return NotFound();
        }

        // Verify the course belongs to this instructor
        if (existingCourse.CreatedBy != instructorId)
        {
            return Forbid();
        }

        // Validate dates if CourseType is Fixed_Time
        if (model.CourseType == CourseType.Fixed_Time)
        {
            if (!model.RegistrationStartDate.HasValue || !model.RegistrationEndDate.HasValue ||
                !model.StartDate.HasValue || !model.EndDate.HasValue)
            {
                ModelState.AddModelError("", "Vui lòng nhập đầy đủ các ngày khi chọn loại khóa học thời gian cố định.");
            }
            else
            {
                if (model.RegistrationStartDate >= model.RegistrationEndDate)
                {
                    ModelState.AddModelError("RegistrationEndDate", "Ngày kết thúc đăng ký phải sau ngày bắt đầu đăng ký.");
                }
                if (model.RegistrationEndDate >= model.StartDate)
                {
                    ModelState.AddModelError("StartDate", "Ngày bắt đầu học phải sau ngày kết thúc đăng ký.");
                }
                if (model.StartDate >= model.EndDate)
                {
                    ModelState.AddModelError("EndDate", "Ngày kết thúc học phải sau ngày bắt đầu học.");
                }
            }
        }

        if (!ModelState.IsValid)
        {
            ViewBag.Categories = await _categoryService.GetAllCategoriesAsync();
            // Lấy danh sách học viên để chọn cho khóa học riêng tư
            var users_edit = await _userService.GetAllUsersAsync();
            ViewBag.Students = users_edit.Where(u => u.UserRoles.Any(ur => ur.Role.Name == "Student")).ToList();
            return View(model);
        }

        existingCourse.Title = model.Title;
        existingCourse.Description = model.Description;
        existingCourse.CategoryId = model.CategoryId;
        existingCourse.CourseStatus = model.Status;
        existingCourse.CourseType = model.CourseType;
        existingCourse.RegistrationStartDate = model.CourseType == CourseType.Fixed_Time ? model.RegistrationStartDate : null;
        existingCourse.RegistrationEndDate = model.CourseType == CourseType.Fixed_Time ? model.RegistrationEndDate : null;
        existingCourse.StartDate = model.CourseType == CourseType.Fixed_Time ? model.StartDate : null;
        existingCourse.EndDate = model.CourseType == CourseType.Fixed_Time ? model.EndDate : null;

        // Handle file upload
        if (thumbnailFile != null && thumbnailFile.Length > 0)
        {
            existingCourse.ThumbnailUrl = await SaveImageAsync(thumbnailFile);
        }
        else if (!string.IsNullOrEmpty(model.ThumbnailUrl))
        {
            existingCourse.ThumbnailUrl = model.ThumbnailUrl;
        }

        await _courseService.UpdateCourseAsync(existingCourse);

        // Xử lý ghi danh học viên nếu là khóa học riêng tư
        if (existingCourse.CourseStatus == CourseStatus.Private)
        {
            var currentEnrollments = await _enrollmentService.GetEnrollmentsByCourseAsync(id);
            var selectedIds = model.SelectedStudentIds ?? new List<int>();

            // 1. Thêm mới các học viên chưa có trong danh sách ghi danh
            foreach (var studentId in selectedIds)
            {
                if (!currentEnrollments.Any(e => e.StudentId == studentId))
                {
                    var enrollment = new Enrollment
                    {
                        CourseId = id,
                        StudentId = studentId,
                        IsMandatory = true,
                        LearningStatus = LearningStatus.NOT_STARTED,
                        EnrolledAt = DateTimeHelper.GetVietnamTimeNow(),
                    };
                    await _enrollmentService.EnrollStudentAsync(enrollment);
                }
            }
            
            // 2. Xóa các học viên KHÔNG còn được chọn (chỉ xóa những enrollment Mandatory để không ảnh hưởng ghi danh tự do nếu có)
            var enrollmentsToRemove = currentEnrollments
                .Where(e => e.IsMandatory && !selectedIds.Contains(e.StudentId))
                .ToList();
                
            foreach (var enrollment in enrollmentsToRemove)
            {
                await _enrollmentService.UnenrollAsync(enrollment.StudentId, id);
            }
        }

        TempData["SuccessMessage"] = "Cập nhật khóa học thành công!";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var instructorId = GetCurrentUserId();
        var course = await _courseService.GetCourseByIdAsync(id);

        if (course == null)
        {
            return NotFound();
        }

        // Verify the course belongs to this instructor
        if (course.CreatedBy != instructorId)
        {
            return Forbid();
        }

        await _courseService.DeleteCourseAsync(id);

        TempData["SuccessMessage"] = "Xóa khóa học thành công!";
        return RedirectToAction(nameof(Index));
    }
}