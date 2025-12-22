using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Online_Course.Data;
using Online_Course.ViewModels;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Online_Course.Areas.Instructor.Controllers;

[Area("Instructor")]
[Authorize(Roles = "Instructor")]
public class ProfileController : Controller
{
    private readonly ApplicationDbContext _context;

    public ProfileController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var userId = GetCurrentUserId();
        if (userId == null) return RedirectToAction("Login", "Account", new { area = "" });

        var user = await _context.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.UserId == userId);

        if (user == null) return NotFound();

        var viewModel = new ProfileViewModel
        {
            UserId = user.UserId,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.UserRoles.FirstOrDefault()?.Role?.Name ?? "Unknown",
            CreatedAt = user.CreatedAt,
            IsActive = user.IsActive
        };

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> Edit()
    {
        var userId = GetCurrentUserId();
        if (userId == null) return RedirectToAction("Login", "Account", new { area = "" });

        var user = await _context.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.UserId == userId);

        if (user == null) return NotFound();

        var viewModel = new ProfileViewModel
        {
            UserId = user.UserId,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.UserRoles.FirstOrDefault()?.Role?.Name ?? "Unknown",
            CreatedAt = user.CreatedAt,
            IsActive = user.IsActive
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ProfileViewModel model)
    {
        var userId = GetCurrentUserId();
        if (userId == null || userId != model.UserId) return Forbid();

        if (!ModelState.IsValid) return View(model);

        var user = await _context.Users.FindAsync(userId);
        if (user == null) return NotFound();

        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == model.Email && u.UserId != userId);
        if (existingUser != null)
        {
            ModelState.AddModelError("Email", "Email đã được sử dụng");
            return View(model);
        }

        user.FullName = model.FullName;
        user.Email = model.Email;
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Cập nhật hồ sơ thành công!";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult ChangePassword()
    {
        return View(new ChangePasswordViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var userId = GetCurrentUserId();
        if (userId == null) return RedirectToAction("Login", "Account", new { area = "" });

        var user = await _context.Users.FindAsync(userId);
        if (user == null) return NotFound();

        if (!VerifyPassword(model.CurrentPassword, user.PasswordHash))
        {
            ModelState.AddModelError("CurrentPassword", "Mật khẩu hiện tại không đúng");
            return View(model);
        }

        user.PasswordHash = HashPassword(model.NewPassword);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Đổi mật khẩu thành công!";
        return RedirectToAction(nameof(Index));
    }

    private int? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : null;
    }

    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }

    private static bool VerifyPassword(string password, string passwordHash)
    {
        return HashPassword(password) == passwordHash;
    }
}
