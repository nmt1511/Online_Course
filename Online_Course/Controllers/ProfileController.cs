using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Online_Course.Data;
using Online_Course.ViewModels;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Online_Course.Controllers;

[Authorize]
public class ProfileController : Controller
{
    private readonly ApplicationDbContext _context;

    public ProfileController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    // Hiển thị trang thông tin cá nhân của người dùng hiện tại
    public async Task<IActionResult> Index()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var user = await _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.UserId == userId);

        if (user == null)
        {
            return NotFound();
        }

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
    // Hiển thị trang chỉnh sửa thông tin cá nhân
    public async Task<IActionResult> Edit()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var user = await _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.UserId == userId);

        if (user == null)
        {
            return NotFound();
        }

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
    // Xử lý cập nhật thông tin cá nhân (chỉ cho phép đổi họ tên)
    public async Task<IActionResult> Edit(ProfileViewModel model)
    {
        var userId = GetCurrentUserId();
        if (userId == null || userId != model.UserId)
        {
            return Forbid();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.UserId == userId);
        if (user == null)
        {
            return NotFound();
        }

        // Chỉ thực hiện cập nhật Họ tên, địa chỉ Email được giữ nguyên theo định danh tài khoản
        user.FullName = model.FullName;

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Cập nhật hồ sơ thành công!";
        return RedirectToAction(nameof(Index));
    }


    [HttpGet]
    // Hiển thị giao diện thay đổi mật khẩu
    public IActionResult ChangePassword()
    {
        return View(new ChangePasswordViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    // Xử lý yêu cầu đổi mật khẩu sau khi xác thực mật khẩu cũ
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            return NotFound();
        }

        // Xác thực mật khẩu hiện tại của người dùng trước khi cho phép thay đổi
        if (!VerifyPassword(model.CurrentPassword, user.PasswordHash))
        {
            ModelState.AddModelError("CurrentPassword", "Mật khẩu hiện tại không đúng");
            return View(model);
        }

        // Cập nhật mật khẩu mới đã được băm an toàn vào cơ sở dữ liệu
        user.PasswordHash = HashPassword(model.NewPassword);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Đổi mật khẩu thành công!";
        return RedirectToAction(nameof(Index));
    }

    // Lấy mã định danh (ID) của người dùng hiện tại từ Claims
    private int? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(userIdClaim, out var userId))
        {
            return userId;
        }
        return null;
    }

    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }

    private static bool VerifyPassword(string password, string passwordHash)
    {
        var hash = HashPassword(password);
        return hash == passwordHash;
    }
}
