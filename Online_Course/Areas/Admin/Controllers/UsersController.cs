using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Online_Course.Models;
using Online_Course.Services;
using Online_Course.ViewModels;

namespace Online_Course.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "AdminOnly")]
public class UsersController : Controller
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    // GET: Admin/Users
    public async Task<IActionResult> Index(string? search, string? role)
    {
        var users = await _userService.GetAllUsersAsync();
        var totalUsers = await _userService.GetTotalUsersCountAsync();
        var instructorCount = await _userService.GetUserCountByRoleAsync("Instructor");
        var studentCount = await _userService.GetUserCountByRoleAsync("Student");

        // Filter by search query
        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.ToLower();
            users = users.Where(u => 
                u.FullName.ToLower().Contains(search) || 
                u.Email.ToLower().Contains(search));
        }

        // Filter by role
        if (!string.IsNullOrWhiteSpace(role))
        {
            users = users.Where(u => u.UserRoles.Any(ur => ur.Role?.Name == role));
        }

        var viewModel = new UserIndexViewModel
        {
            Users = users.Select(u => new UserListViewModel
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
            SelectedRole = role
        };

        return View(viewModel);
    }

    // GET: Admin/Users/Create
    public async Task<IActionResult> Create()
    {
        var roles = await _userService.GetAllRolesAsync();
        ViewBag.Roles = new SelectList(roles, "RoleId", "Name");
        return View();
    }


    // POST: Admin/Users/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateUserViewModel model)
    {
        if (ModelState.IsValid)
        {
            // Check if email already exists
            var existingUser = await _userService.GetUserByEmailAsync(model.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError("Email", "Email already exists");
                var roles = await _userService.GetAllRolesAsync();
                ViewBag.Roles = new SelectList(roles, "RoleId", "Name");
                return View(model);
            }

            var user = new User
            {
                FullName = model.FullName,
                Email = model.Email
            };

            await _userService.CreateUserAsync(user, model.Password, model.RoleId);
            TempData["SuccessMessage"] = "User created successfully!";
            return RedirectToAction(nameof(Index));
        }

        var allRoles = await _userService.GetAllRolesAsync();
        ViewBag.Roles = new SelectList(allRoles, "RoleId", "Name");
        return View(model);
    }

    // GET: Admin/Users/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        var courses = await _userService.GetCoursesByUserAsync(id);
        var roleName = user.UserRoles.FirstOrDefault()?.Role?.Name ?? "No Role";

        var viewModel = new UserDetailsViewModel
        {
            UserId = user.UserId,
            FullName = user.FullName,
            Email = user.Email,
            RoleName = roleName,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            Courses = courses.Select(c => new UserCourseViewModel
            {
                CourseId = c.CourseId,
                Title = c.Title,
                Category = c.Category,
                Status = c.Status,
                EnrollmentCount = c.Enrollments?.Count ?? 0,
                LessonCount = c.Lessons?.Count ?? 0
            }).ToList(),
            TotalStudents = courses.Sum(c => c.Enrollments?.Count ?? 0),
            TotalLessons = courses.Sum(c => c.Lessons?.Count ?? 0)
        };

        return View(viewModel);
    }

    // GET: Admin/Users/Edit/5
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
        ViewBag.Roles = new SelectList(roles, "RoleId", "Name", viewModel.RoleId);
        return View(viewModel);
    }

    // POST: Admin/Users/Edit/5
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

            // Check if email is taken by another user
            var existingUser = await _userService.GetUserByEmailAsync(model.Email);
            if (existingUser != null && existingUser.UserId != id)
            {
                ModelState.AddModelError("Email", "Email already exists");
                var roles = await _userService.GetAllRolesAsync();
                ViewBag.Roles = new SelectList(roles, "RoleId", "Name", model.RoleId);
                return View(model);
            }

            user.FullName = model.FullName;
            user.Email = model.Email;
            user.IsActive = model.IsActive;

            await _userService.UpdateUserAsync(user);
            await _userService.AssignRoleAsync(id, model.RoleId);

            TempData["SuccessMessage"] = "User updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        var allRoles = await _userService.GetAllRolesAsync();
        ViewBag.Roles = new SelectList(allRoles, "RoleId", "Name", model.RoleId);
        return View(model);
    }

    // POST: Admin/Users/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _userService.DeleteUserAsync(id);
        TempData["SuccessMessage"] = "User deleted successfully!";
        return RedirectToAction(nameof(Index));
    }
}
