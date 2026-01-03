using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Online_Course.Models;
using Online_Course.Services.ProgressService;
using Online_Course.Services.UserService;
using Online_Course.ViewModels;

namespace Online_Course.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "AdminOnly")]
public class UsersController : Controller
{
    private readonly IUserService _userService;
    private readonly IProgressService _progressService;

    public UsersController(IUserService userService, IProgressService progressService)
    {
        _userService = userService;
        _progressService = progressService;
    }

    // Displays the user management list with search, role filtering, and pagination functions.
    public async Task<IActionResult> Index(string? search, string? role, int page = 1)
    {
        // Pagination settings
        const int pageSize = 10;
        
        var users = await _userService.GetAllUsersAsync();
        var totalUsers = await _userService.GetTotalUsersCountAsync();
        var instructorCount = await _userService.GetUserCountByRoleAsync("Instructor");
        var studentCount = await _userService.GetUserCountByRoleAsync("Student");

        // Filters the user list based on search keywords (Full Name or Email).
        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.ToLower();
            users = users.Where(u => 
                u.FullName.ToLower().Contains(search) || 
                u.Email.ToLower().Contains(search));
        }

        // Filters the list by user role (excluding the Administrator role).
        if (!string.IsNullOrWhiteSpace(role) && role != "Admin")
        {
            users = users.Where(u => u.UserRoles.Any(ur => ur.Role?.Name == role));
        }

        // Calculate pagination
        var totalFilteredUsers = users.Count();
        var totalPages = (int)Math.Ceiling(totalFilteredUsers / (double)pageSize);
        
        // Validate page number
        if (page < 1) page = 1;
        if (page > totalPages && totalPages > 0) page = totalPages;
        
        // Apply pagination
        var paginatedUsers = users
            .Skip((page - 1) * pageSize)
            .Take(pageSize);

        var viewModel = new UserIndexViewModel
        {
            Users = paginatedUsers.Select(u => new UserListViewModel
            {
                UserId = u.UserId,
                FullName = u.FullName,
                Email = u.Email,
                RoleName = u.UserRoles.FirstOrDefault()?.Role?.Name ?? "No Role",
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt
            }),
            TotalUsers = totalUsers,
            InstructorCount = instructorCount,
            StudentCount = studentCount,
            SearchQuery = search,
            SelectedRole = role,
            CurrentPage = page,
            PageSize = pageSize,
            TotalPages = totalPages
        };

        return View(viewModel);
    }

    // Displays the interface for creating a new user account.
    public async Task<IActionResult> Create()
    {
        var roles = await _userService.GetAllRolesAsync();
        // Excludes the Administrator role from the selection list when creating a new user.
        var filteredRoles = roles.Where(r => r.Name != "Admin");
        ViewBag.Roles = new SelectList(filteredRoles, "RoleId", "Name");
        return View();
    }


    // Xử lý logic tạo mới tài khoản sau khi xác thực dữ liệu đầu vào
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateUserViewModel model)
    {
        if (ModelState.IsValid)
        {
            // Đảm bảo không cho phép tạo tài khoản có vai trò Quản trị viên từ giao diện này
            var allRoles = await _userService.GetAllRolesAsync();
            var adminRole = allRoles.FirstOrDefault(r => r.Name == "Admin");
            if (adminRole != null && model.RoleId == adminRole.RoleId)
            {
                ModelState.AddModelError("RoleId", "Không thể tạo người dùng với vai trò Admin");
                var filteredRoles = allRoles.Where(r => r.Name != "Admin");
                ViewBag.Roles = new SelectList(filteredRoles, "RoleId", "Name");
                return View(model);
            }
            
            // Xác thực xem địa chỉ Email đã tồn tại trong hệ thống hay chưa
            var existingUser = await _userService.GetUserByEmailAsync(model.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError("Email", "Email đã tồn tại");
                var filteredRoles = allRoles.Where(r => r.Name != "Admin");
                ViewBag.Roles = new SelectList(filteredRoles, "RoleId", "Name");
                return View(model);
            }

            var user = new User
            {
                FullName = model.FullName,
                Email = model.Email
            };

            await _userService.CreateUserAsync(user, model.Password, model.RoleId);
            TempData["SuccessMessage"] = "Tạo người dùng thành công!";
            return RedirectToAction(nameof(Index));
        }

        var roles = await _userService.GetAllRolesAsync();
        // Exclude Admin role from selection
        var filteredRolesForView = roles.Where(r => r.Name != "Admin");
        ViewBag.Roles = new SelectList(filteredRolesForView, "RoleId", "Name");
        return View(model);
    }

    // Hiển thị thông tin chi tiết của người dùng kèm theo danh sách khóa học liên quan (nếu là Giảng viên hoặc Học viên)
    public async Task<IActionResult> Details(int id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        var roleName = user.UserRoles.FirstOrDefault()?.Role?.Name ?? "No Role";
        var viewModel = new UserDetailsViewModel
        {
            UserId = user.UserId,
            FullName = user.FullName,
            Email = user.Email,
            RoleName = roleName,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        };

        if (roleName == "Instructor")
        {
            // Logic cho Giảng viên: Lấy các khóa học đã tạo
            var courses = await _userService.GetCoursesByUserAsync(id);

            var userCourseViewModels = new List<UserCourseViewModel>();

            foreach (var c in courses)
            {
                userCourseViewModels.Add(new UserCourseViewModel
                {
                    CourseId = c.CourseId,
                    Title = c.Title,
                    CategoryId = c.CategoryId,
                    CategoryName = c.CategoryEntity?.Name ?? "Chưa phân loại",
                    Status = c.CourseStatus,
                    EnrollmentCount = c.Enrollments?.Count ?? 0,
                    LessonCount = c.Lessons?.Count ?? 0
                });
            }

            viewModel.Courses = userCourseViewModels;
            
            viewModel.TotalStudents = viewModel.Courses.Sum(c => c.EnrollmentCount);
            viewModel.TotalLessons = viewModel.Courses.Sum(c => c.LessonCount);
        }
        else if (roleName == "Student")
        {
            // Logic cho Học viên: Lấy các khóa học đã đăng ký
            var enrollments = await _userService.GetStudentEnrollmentsAsync(id);
            var enrollmentViewModels = new List<UserEnrollmentViewModel>();

            foreach (var enrollment in enrollments)
            {
                var completedLessons = await _progressService.GetCompletedLessonsCountAsync(id, enrollment.CourseId);
                
                enrollmentViewModels.Add(new UserEnrollmentViewModel
                {
                    CourseId = enrollment.CourseId,
                    Title = enrollment.Course.Title,
                    CategoryName = enrollment.Course.CategoryEntity?.Name ?? "Chưa phân loại",
                    ThumbnailUrl = enrollment.Course.ThumbnailUrl,
                    EnrolledAt = enrollment.EnrolledAt,
                    LearningStatus = enrollment.LearningStatus,
                    ProgressPercent = enrollment.ProgressPercent,
                    CompletedLessons = completedLessons,
                    TotalLessons = enrollment.Course.Lessons.Count
                });
            }
            
            viewModel.Enrollments = enrollmentViewModels;
        }

        return View(viewModel);
    }

    // Hiển thị giao diện điều chỉnh thông tin tài khoản người dùng hiện có
    public async Task<IActionResult> Edit(int id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        var viewModel = new EditUserViewModel
        {
            UserId = user.UserId,
            FullName = user.FullName,
            Email = user.Email,
            RoleId = user.UserRoles.FirstOrDefault()?.RoleId ?? 0,
            IsActive = user.IsActive
        };

        var roles = await _userService.GetAllRolesAsync();
        // Exclude Admin role from selection
        var filteredRoles = roles.Where(r => r.Name != "Admin");
        ViewBag.Roles = new SelectList(filteredRoles, "RoleId", "Name", viewModel.RoleId);
        return View(viewModel);
    }

    // Xử lý cập nhật thông tin thay đổi của người dùng và gán lại vai trò tương ứng (nếu có)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, EditUserViewModel model)
    {
        if (id != model.UserId)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Ngăn chặn việc gán vai trò Quản trị viên cho các tài khoản thông thường
            var allRoles = await _userService.GetAllRolesAsync();
            var adminRole = allRoles.FirstOrDefault(r => r.Name == "Admin");
            if (adminRole != null && model.RoleId == adminRole.RoleId)
            {
                ModelState.AddModelError("RoleId", "Không thể gán vai trò Admin");
                var filteredRoles = allRoles.Where(r => r.Name != "Admin");
                ViewBag.Roles = new SelectList(filteredRoles, "RoleId", "Name", model.RoleId);
                return View(model);
            }

            // Kiểm tra xem địa chỉ Email mới có bị trùng lặp với người dùng khác hay không
            var existingUser = await _userService.GetUserByEmailAsync(model.Email);
            if (existingUser != null && existingUser.UserId != id)
            {
                ModelState.AddModelError("Email", "Email đã tồn tại");
                var filteredRoles = allRoles.Where(r => r.Name != "Admin");
                ViewBag.Roles = new SelectList(filteredRoles, "RoleId", "Name", model.RoleId);
                return View(model);
            }

            user.FullName = model.FullName;
            user.Email = model.Email;
            user.IsActive = model.IsActive;

            await _userService.UpdateUserAsync(user);
            await _userService.AssignRoleAsync(id, model.RoleId);

            TempData["SuccessMessage"] = "Cập nhật thông tin người dùng thành công!";
            return RedirectToAction(nameof(Index));
        }

        var roles = await _userService.GetAllRolesAsync();
        // Exclude Admin role from selection
        var filteredRolesForView = roles.Where(r => r.Name != "Admin");
        ViewBag.Roles = new SelectList(filteredRolesForView, "RoleId", "Name", model.RoleId);
        return View(model);
    }

    // Loại bỏ tài khoản người dùng khỏi hệ thống dữ liệu của ứng dụng
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _userService.DeleteUserAsync(id);
        TempData["SuccessMessage"] = "Xóa người dùng thành công!";
        return RedirectToAction(nameof(Index));
    }
}
