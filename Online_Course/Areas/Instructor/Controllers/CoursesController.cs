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
    private readonly IWebHostEnvironment _webHostEnvironment;

    public CoursesController(
        ICourseService courseService,
        IEnrollmentService enrollmentService,
        ICategoryService categoryService,
        IWebHostEnvironment webHostEnvironment)
    {
        _courseService = courseService;
        _enrollmentService = enrollmentService;
        _categoryService = categoryService;
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
        if (!ModelState.IsValid)
        {
            ViewBag.Categories = await _categoryService.GetAllCategoriesAsync();
            return View(model);
        }

        var instructorId = GetCurrentUserId();
        var thumbnailUrl = model.ThumbnailUrl;

        // Handle file upload
        if (thumbnailFile != null && thumbnailFile.Length > 0)
        {
            thumbnailUrl = await SaveImageAsync(thumbnailFile);
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
            ThumbnailUrl = thumbnailUrl ?? "",
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
