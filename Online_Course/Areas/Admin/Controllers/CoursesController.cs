using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Online_Course.Models;
using Online_Course.Services;
using Online_Course.ViewModels;

namespace Online_Course.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "AdminOnly")]
public class CoursesController : Controller
{
    private readonly ICourseService _courseService;
    private readonly IUserService _userService;

    public CoursesController(ICourseService courseService, IUserService userService)
    {
        _courseService = courseService;
        _userService = userService;
    }

    // GET: Admin/Courses
    public async Task<IActionResult> Index(string? category, string? status, string? search)
    {
        var courses = await _courseService.GetAllCoursesAsync();
        var totalCourses = await _courseService.GetTotalCoursesCountAsync();
        var publishedCourses = await _courseService.GetPublishedCoursesCountAsync();

        // Apply filters
        if (!string.IsNullOrEmpty(category))
        {
            courses = courses.Where(c => c.Category == category);
        }

        if (!string.IsNullOrEmpty(status))
        {
            if (status == "published")
                courses = courses.Where(c => c.Status == CourseStatus.Public);
            else if (status == "draft")
                courses = courses.Where(c => c.Status == CourseStatus.Draft);
            else if (status == "private")
                courses = courses.Where(c => c.Status == CourseStatus.Private);
        }

        if (!string.IsNullOrEmpty(search))
        {
            courses = courses.Where(c => 
                c.Title.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                c.Description.Contains(search, StringComparison.OrdinalIgnoreCase));
        }


        var viewModel = new CourseIndexViewModel
        {
            Courses = courses.Select(c => new CourseListViewModel
            {
                CourseId = c.CourseId,
                Title = c.Title,
                Description = c.Description,
                Category = c.Category,
                ThumbnailUrl = c.ThumbnailUrl,
                InstructorName = c.Instructor?.FullName ?? "Not Assigned",
                InstructorId = c.CreatedBy,
                Status = c.Status,
                EnrollmentCount = c.Enrollments?.Count ?? 0
            }),
            TotalCourses = totalCourses,
            PublishedCourses = publishedCourses,
            DraftCourses = totalCourses - publishedCourses,
            CategoryFilter = category,
            StatusFilter = status,
            SearchQuery = search
        };

        // Get categories for filter dropdown
        var categories = await _courseService.GetAllCategoriesAsync();
        ViewBag.Categories = categories;

        return View(viewModel);
    }

    // GET: Admin/Courses/Create
    public async Task<IActionResult> Create()
    {
        await PopulateInstructorsDropdown();
        return View();
    }

    // POST: Admin/Courses/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateCourseViewModel model)
    {
        if (ModelState.IsValid)
        {
            var course = new Course
            {
                Title = model.Title,
                Description = model.Description,
                Category = model.Category,
                ThumbnailUrl = model.ThumbnailUrl,
                CreatedBy = model.InstructorId,
                Status = model.Status
            };

            await _courseService.CreateCourseAsync(course);
            TempData["SuccessMessage"] = "Course created successfully!";
            return RedirectToAction(nameof(Index));
        }

        await PopulateInstructorsDropdown();
        return View(model);
    }


    // GET: Admin/Courses/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var course = await _courseService.GetCourseByIdAsync(id);
        if (course == null)
        {
            return NotFound();
        }

        var viewModel = new EditCourseViewModel
        {
            CourseId = course.CourseId,
            Title = course.Title,
            Description = course.Description,
            Category = course.Category,
            ThumbnailUrl = course.ThumbnailUrl,
            InstructorId = course.CreatedBy,
            Status = course.Status
        };

        await PopulateInstructorsDropdown(course.CreatedBy);
        return View(viewModel);
    }

    // POST: Admin/Courses/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, EditCourseViewModel model)
    {
        if (id != model.CourseId)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            var course = await _courseService.GetCourseByIdAsync(id);
            if (course == null)
            {
                return NotFound();
            }

            course.Title = model.Title;
            course.Description = model.Description;
            course.Category = model.Category;
            course.ThumbnailUrl = model.ThumbnailUrl;
            course.CreatedBy = model.InstructorId;
            course.Status = model.Status;

            await _courseService.UpdateCourseAsync(course);
            TempData["SuccessMessage"] = "Course updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        await PopulateInstructorsDropdown(model.InstructorId);
        return View(model);
    }

    // POST: Admin/Courses/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _courseService.DeleteCourseAsync(id);
        TempData["SuccessMessage"] = "Course deleted successfully!";
        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateInstructorsDropdown(int? selectedId = null)
    {
        var users = await _userService.GetAllUsersAsync();
        var instructors = users.Where(u => u.UserRoles.Any(ur => ur.Role.Name == "Instructor"));
        ViewBag.Instructors = new SelectList(instructors, "UserId", "FullName", selectedId);
    }
}
