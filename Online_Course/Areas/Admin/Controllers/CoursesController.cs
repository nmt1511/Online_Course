using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Online_Course.Helper;
using Online_Course.Models;
using Online_Course.Services;
using Online_Course.ViewModels;

namespace Online_Course.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "AdminOnly")]
public class CoursesController : Controller
{
    private readonly ICourseService _courseService;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly IUserService _userService;
    private readonly IEnrollmentService _enrollmentService;
    private readonly IProgressService _progressService;

    public CoursesController(
        ICourseService courseService, 
        IUserService userService,
        IEnrollmentService enrollmentService,
        IProgressService progressService,
        IWebHostEnvironment webHostEnvironment)
    {
        _courseService = courseService;
        _userService = userService;
        _enrollmentService = enrollmentService;
        _progressService = progressService;
        _webHostEnvironment = webHostEnvironment;
    }

    // GET: Admin/Courses
    public async Task<IActionResult> Index(int? category, string? status, string? search, int page = 1)
    {
        var courses = await _courseService.GetAllCoursesAsync();
        
        // Apply filters
        if (category.HasValue)
        {
            courses = courses.Where(c => c.CategoryId == category.Value);
        }

        if (!string.IsNullOrEmpty(status))
        {
            if (status == "published")
                courses = courses.Where(c => c.CourseStatus == CourseStatus.Public);
            else if (status == "draft")
                courses = courses.Where(c => c.CourseStatus == CourseStatus.Draft);
            else if (status == "private")
                courses = courses.Where(c => c.CourseStatus == CourseStatus.Private);
        }

        if (!string.IsNullOrEmpty(search))
        {
            courses = courses.Where(c => 
                c.Title.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                c.Description.Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        // Pagination logic
        int pageSize = 10;
        int totalItems = courses.Count();
        int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
        
        // Ensure page is within valid range
        page = Math.Max(1, Math.Min(page, totalPages > 0 ? totalPages : 1));

        var pagedCourses = courses
            .OrderByDescending(c => c.CourseId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize);

        var totalCoursesCount = await _courseService.GetTotalCoursesCountAsync();
        var publishedCoursesCount = await _courseService.GetPublishedCoursesCountAsync();
        var draftCourseCount = await _courseService.GetDraftCoursesCountAsync();

        var viewModel = new CourseIndexViewModel
        {
            Courses = pagedCourses.Select(c => new CourseListViewModel
            {
                CourseId = c.CourseId,
                Title = c.Title,
                Description = c.Description,
                CategoryId = c.CategoryId,
                CategoryName = c.CategoryEntity?.Name ?? "Chưa phân loại",
                ThumbnailUrl = c.ThumbnailUrl,
                InstructorName = c.Instructor?.FullName ?? "Not Assigned",
                InstructorId = c.CreatedBy,
                Status = c.CourseStatus,
                EnrollmentCount = c.Enrollments?.Count ?? 0
            }),
            TotalCourses = totalCoursesCount,
            PublishedCourses = publishedCoursesCount,
            DraftCourses = draftCourseCount,
            CategoryFilter = category,
            StatusFilter = status,
            SearchQuery = search,
            CurrentPage = page,
            TotalPages = totalPages
        };

        // Get categories for filter dropdown
        var categories = await _courseService.GetAllCategoriesAsync();
        ViewBag.Categories = categories;

        return View(viewModel);
    }

    // GET: Admin/Courses/Details/5
    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var course = await _courseService.GetCourseByIdAsync(id);

        if (course == null)
        {
            return NotFound();
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
            InstructorName = course.Instructor?.FullName ?? "Chưa phân công",
            ShowInstructor = true, // Admin can see instructor name
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

    // GET: Admin/Courses/Create
    public async Task<IActionResult> Create()
    {
        await PopulateInstructorsDropdown();
        await PopulateCategoriesDropdown();
        await PopulateStudentsViewBag();
        return View(new CreateCourseViewModel());
    }

    // POST: Admin/Courses/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateCourseViewModel model, IFormFile? thumbnailFile)
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

        if (ModelState.IsValid)
        {
            string thumbnailUrl;

            // Handle file upload first
            if (thumbnailFile != null && thumbnailFile.Length > 0)
            {
                // Check file size (5MB max)
                if (thumbnailFile.Length > 5 * 1024 * 1024)
                {
                    ModelState.AddModelError("thumbnailFile", "Kích thước file vượt quá 5MB");
                    await PopulateInstructorsDropdown(model.InstructorId);
                    await PopulateCategoriesDropdown(model.CategoryId);
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
                CreatedBy = model.InstructorId,
                CourseStatus = model.Status,
                CourseType = model.CourseType,
                RegistrationStartDate = model.CourseType == CourseType.Fixed_Time ? model.RegistrationStartDate : null,
                RegistrationEndDate = model.CourseType == CourseType.Fixed_Time ? model.RegistrationEndDate : null,
                StartDate = model.CourseType == CourseType.Fixed_Time ? model.StartDate : null,
                EndDate = model.CourseType == CourseType.Fixed_Time ? model.EndDate : null
            };

            await _courseService.CreateCourseAsync(course);

            // Process mandatory enrollments for private courses
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

            TempData["SuccessMessage"] = "Khóa học đã được tạo thành công!";
            return RedirectToAction(nameof(Index));
        }

        await PopulateInstructorsDropdown(model.InstructorId);
        await PopulateCategoriesDropdown(model.CategoryId);
        await PopulateStudentsViewBag();
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
            CategoryId = course.CategoryId,
            ThumbnailUrl = course.ThumbnailUrl,
            InstructorId = course.CreatedBy,
            Status = course.CourseStatus,
            CourseType = course.CourseType,
            RegistrationStartDate = course.RegistrationStartDate,
            RegistrationEndDate = course.RegistrationEndDate,
            StartDate = course.StartDate,
            EndDate = course.EndDate,
            SelectedStudentIds = course.Enrollments?.Where(e => e.IsMandatory).Select(e => e.StudentId).ToList() ?? new List<int>()
        };

        await PopulateInstructorsDropdown(course.CreatedBy);
        await PopulateCategoriesDropdown(course.CategoryId);
        await PopulateStudentsViewBag();
        return View(viewModel);
    }

    // POST: Admin/Courses/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, EditCourseViewModel model, IFormFile? thumbnailFile)
    {
        if (id != model.CourseId)
        {
            return NotFound();
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

        if (ModelState.IsValid)
        {
            var course = await _courseService.GetCourseByIdAsync(id);
            if (course == null)
            {
                return NotFound();
            }

            course.Title = model.Title;
            course.Description = model.Description;
            course.CategoryId = model.CategoryId;
            course.CreatedBy = model.InstructorId;
            course.CourseStatus = model.Status;
            course.CourseType = model.CourseType;
            course.RegistrationStartDate = model.CourseType == CourseType.Fixed_Time ? model.RegistrationStartDate : null;
            course.RegistrationEndDate = model.CourseType == CourseType.Fixed_Time ? model.RegistrationEndDate : null;
            course.StartDate = model.CourseType == CourseType.Fixed_Time ? model.StartDate : null;
            course.EndDate = model.CourseType == CourseType.Fixed_Time ? model.EndDate : null;

            // Handle file upload
            if (thumbnailFile != null && thumbnailFile.Length > 0)
            {
                course.ThumbnailUrl = await SaveImageAsync(thumbnailFile);
            }
            else if (!string.IsNullOrEmpty(model.ThumbnailUrl))
            {
                course.ThumbnailUrl = model.ThumbnailUrl;
            }

            await _courseService.UpdateCourseAsync(course);

            // Process mandatory enrollments for private courses
            if (course.CourseStatus == CourseStatus.Private)
            {
                var currentEnrollments = await _enrollmentService.GetEnrollmentsByCourseAsync(id);
                var selectedIds = model.SelectedStudentIds ?? new List<int>();

                // Add new enrollments
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

                // Remove unselected mandatory enrollments
                var enrollmentsToRemove = currentEnrollments
                    .Where(e => e.IsMandatory && !selectedIds.Contains(e.StudentId))
                    .ToList();

                foreach (var enrollment in enrollmentsToRemove)
                {
                    await _enrollmentService.UnenrollAsync(enrollment.StudentId, id);
                }
            }

            TempData["SuccessMessage"] = "Khóa học đã được cập nhật thành công!";
            return RedirectToAction(nameof(Index));
        }

        await PopulateInstructorsDropdown(model.InstructorId);
        await PopulateCategoriesDropdown(model.CategoryId);
        await PopulateStudentsViewBag();
        return View(model);
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

    // POST: Admin/Courses/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _courseService.DeleteCourseAsync(id);
        TempData["SuccessMessage"] = "Khóa học đã được xóa thành công!";
        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateInstructorsDropdown(int? selectedId = null)
    {
        var users = await _userService.GetAllUsersAsync();
        var instructors = users.Where(u => u.UserRoles.Any(ur => ur.Role.Name == "Instructor"));
        ViewBag.Instructors = new SelectList(instructors, "UserId", "FullName", selectedId);
    }

    private async Task PopulateCategoriesDropdown(int? selectedId = null)
    {
        var categories = await _courseService.GetAllCategoriesAsync();
        ViewBag.CategoriesList = new SelectList(categories, "CategoryId", "Name", selectedId);
        ViewBag.Categories = categories; // Also store as list for raw iteration
    }

    private async Task PopulateStudentsViewBag()
    {
        var users = await _userService.GetAllUsersAsync();
        ViewBag.Students = users.Where(u => u.UserRoles.Any(ur => ur.Role.Name == "Student")).ToList();
    }
}

