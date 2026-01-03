using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Online_Course.Helper;
using Online_Course.Models;
using Online_Course.Services.CourseService;
using Online_Course.Services.EnrollmentService;
using Online_Course.Services.ProgressService;
using Online_Course.Services.UserService;
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

    // Hiển thị danh sách tất cả các khóa học kèm theo bộ lọc thông tin và phân trang
    public async Task<IActionResult> Index(int? category, string? status, string? search, int page = 1)
    {
        var courses = await _courseService.GetAllCoursesAsync();
        
        // Thực hiện áp dụng các tiêu chí lọc dữ liệu
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

        // Thiết lập các thông số phân trang cho danh sách khóa học
        int pageSize = 10;
        int totalItems = courses.Count();
        int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
        
        // Đảm bảo chỉ số trang nằm trong phạm vi hợp lệ
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

        // Truy xuất danh sách danh mục để phục vụ bộ lọc tìm kiếm trên giao diện
        var categories = await _courseService.GetAllCategoriesAsync();
        ViewBag.Categories = categories;

        return View(viewModel);
    }

    // Hiển thị chi tiết thông tin khóa học, danh sách bài học và danh sách học viên đăng ký
    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var course = await _courseService.GetCourseByIdAsync(id);

        if (course == null)
        {
            return NotFound();
        }

        // Truy xuất danh sách học viên đã đăng ký khóa học này
        var enrollments = await _enrollmentService.GetEnrollmentsByCourseAsync(id);

        // Xây dựng danh sách học viên kèm theo tỷ lệ phần trăm hoàn thành bài học
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

    // Hiển thị giao diện khởi tạo khóa học mới
    public async Task<IActionResult> Create()
    {
        await PopulateInstructorsDropdown();
        await PopulateCategoriesDropdown();
        await PopulateStudentsViewBag();
        return View(new CreateCourseViewModel());
    }

    // Xử lý logic khởi tạo khóa học mới kèm theo tệp tin hình ảnh thu nhỏ
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateCourseViewModel model, IFormFile? thumbnailFile)
    {
        // Loại bỏ ThumbnailUrl khỏi kiểm tra ModelState vì dữ liệu này có thể được truyền qua tệp tin tải lên
        ModelState.Remove("ThumbnailUrl");

        // Kiểm tra tính hợp lệ của các mốc thời gian nếu khóa học có định dạng thời gian cố định
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

            // Ưu tiên xử lý lưu tệp tin hình ảnh tải lên từ máy tính
            if (thumbnailFile != null && thumbnailFile.Length > 0)
            {
                // Giới hạn kích thước tệp tin hình ảnh tải lên (tối đa 5MB)
                if (thumbnailFile.Length > 5 * 1024 * 1024)
                {
                    ModelState.AddModelError("thumbnailFile", "Kích thước file vượt quá 5MB");
                    await PopulateInstructorsDropdown(model.InstructorId);
                    await PopulateCategoriesDropdown(model.CategoryId);
                    return View(model);
                }
                thumbnailUrl = await SaveImageAsync(thumbnailFile);
            }
            // Sử dụng đường dẫn URL nếu không có tệp tin tải lên trực tiếp
            else if (!string.IsNullOrWhiteSpace(model.ThumbnailUrl))
            {
                thumbnailUrl = model.ThumbnailUrl;
            }
            // Sử dụng hình ảnh mặc định nếu không có bất kỳ nguồn hình ảnh nào được chỉ định
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

            // Tự động ghi danh sinh viên nếu khóa học được thiết lập ở trạng thái Riêng tư
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


    // Hiển thị giao diện cập nhật thông tin khóa học hiện có
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

    // Xử lý cập nhật thông tin thay đổi của khóa học
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

            // Xử lý tệp tin hình ảnh thu nhỏ mới nếu có tải lên thay thế
            if (thumbnailFile != null && thumbnailFile.Length > 0)
            {
                course.ThumbnailUrl = await SaveImageAsync(thumbnailFile);
            }
            else if (!string.IsNullOrEmpty(model.ThumbnailUrl))
            {
                course.ThumbnailUrl = model.ThumbnailUrl;
            }

            await _courseService.UpdateCourseAsync(course);

            // Cập nhật lại danh sách sinh viên ghi danh bắt buộc đối với các khóa học riêng tư
            if (course.CourseStatus == CourseStatus.Private)
            {
                var currentEnrollments = await _enrollmentService.GetEnrollmentsByCourseAsync(id);
                var selectedIds = model.SelectedStudentIds ?? new List<int>();

                // Bổ sung các sinh viên mới vào danh sách ghi danh
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

                // Loại bỏ các sinh viên không còn nằm trong danh sách ghi danh bắt buộc
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
        // Khởi tạo thư mục lưu trữ hình ảnh nếu chưa tồn tại trong hệ thống
        var imagesFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "courses");
        if (!Directory.Exists(imagesFolder))
        {
            Directory.CreateDirectory(imagesFolder);
        }

        // Khởi tạo tên tệp tin duy nhất nhằm tránh xung đột dữ liệu trên máy chủ
        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var filePath = Path.Combine(imagesFolder, fileName);

        // Save file
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // Trả về đường dẫn tương đối của hình ảnh để lưu trữ vào cơ sở dữ liệu
        return $"/images/courses/{fileName}";
    }

    // Loại bỏ hoàn toàn khóa học khỏi hệ thống sau khi xác thực các điều kiện xóa
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var course = await _courseService.GetCourseByIdAsync(id);
        if (course == null)
        {
            return NotFound();
        }

        // Kiểm tra xem khóa học có học viên nào đang đăng ký hay không
        var enrollmentCount = course.Enrollments?.Count ?? 0;
        if (enrollmentCount > 0)
        {
            TempData["ErrorMessage"] = "Không thể xóa khóa học đã có học viên đăng ký.";
            return RedirectToAction(nameof(Index));
        }

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

