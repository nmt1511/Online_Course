using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
    private readonly IWebHostEnvironment _webHostEnvironment;

    public CoursesController(
        ICourseService courseService,
        IEnrollmentService enrollmentService,
        ICategoryService categoryService,
        IProgressService progressService,
        IWebHostEnvironment webHostEnvironment)
    {
        _courseService = courseService;
        _enrollmentService = enrollmentService;
        _categoryService = categoryService;
        _progressService = progressService;
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
            Category = c.Category,
            ThumbnailUrl = c.ThumbnailUrl,
            Status = c.Status,
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
        return View(new InstructorCreateCourseViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(InstructorCreateCourseViewModel model, IFormFile? thumbnailFile)
    {
        // Remove ThumbnailUrl from validation - it's optional
        ModelState.Remove("ThumbnailUrl");
        
        if (!ModelState.IsValid)
        {
            ViewBag.Categories = await _categoryService.GetAllCategoriesAsync();
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

        // Find CategoryId from Category name
        var categories = await _categoryService.GetAllCategoriesAsync();
        var category = categories.FirstOrDefault(c => c.Name == model.Category);
        
        var course = new Course
        {
            Title = model.Title,
            Description = model.Description,
            Category = model.Category,
            CategoryId = category?.CategoryId,
            ThumbnailUrl = thumbnailUrl,
            Status = model.Status,
            CreatedBy = instructorId
        };

        await _courseService.CreateCourseAsync(course);

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
            Category = course.Category,
            ThumbnailUrl = course.ThumbnailUrl,
            Status = course.Status,
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
                LessonType = DetectLessonType(l.ContentUrl)
            }) ?? Enumerable.Empty<LessonSummaryViewModel>(),
            Students = students
        };

        return View(viewModel);
    }

    private string DetectLessonType(string url)
    {
        if (string.IsNullOrEmpty(url)) return "video";
        
        var lowerUrl = url.ToLower();
        if (lowerUrl.EndsWith(".pdf") || lowerUrl.Contains("pdf"))
            return "pdf";
        
        return "video";
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
            Category = course.Category,
            ThumbnailUrl = course.ThumbnailUrl,
            Status = course.Status,
            InstructorName = course.Instructor?.FullName ?? "",
            EnrollmentCount = course.Enrollments?.Count ?? 0,
            LessonCount = course.Lessons?.Count ?? 0
        };

        ViewBag.Categories = await _categoryService.GetAllCategoriesAsync();
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

        if (!ModelState.IsValid)
        {
            ViewBag.Categories = await _categoryService.GetAllCategoriesAsync();
            return View(model);
        }

        // Find CategoryId from Category name
        var categories = await _categoryService.GetAllCategoriesAsync();
        var category = categories.FirstOrDefault(c => c.Name == model.Category);
        
        existingCourse.Title = model.Title;
        existingCourse.Description = model.Description;
        existingCourse.Category = model.Category;
        existingCourse.CategoryId = category?.CategoryId;
        existingCourse.Status = model.Status;

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
