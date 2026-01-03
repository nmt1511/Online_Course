using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Online_Course.Data;
using Online_Course.Helper;
using Online_Course.Models;
using Online_Course.ViewModels;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Online_Course.Controllers;

public class AccountController : Controller
{
    private readonly ApplicationDbContext _context;

    public AccountController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToRoleDashboard();
        }

        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Email == model.Email && u.IsActive);

        if (user == null || !VerifyPassword(model.Password, user.PasswordHash))
        {
            ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không đúng");
            return View(model);
        }

        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim("UserId", user.UserId.ToString()),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Email, user.Email)
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = model.RememberMe,
            ExpiresUtc = DateTimeOffset.UtcNow.AddHours(24)
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties);

        if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
        {
            return Redirect(model.ReturnUrl);
        }

        return RedirectToRoleDashboard(roles.FirstOrDefault());
    }

    [HttpGet]
    public IActionResult Register()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToRoleDashboard();
        }

        return View(new RegisterViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (await _context.Users.AnyAsync(u => u.Email == model.Email))
        {
            ModelState.AddModelError("Email", "Email đã được sử dụng");
            return View(model);
        }

        var studentRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Student");
        if (studentRole == null)
        {
            ModelState.AddModelError(string.Empty, "Lỗi hệ thống. Vui lòng thử lại sau.");
            return View(model);
        }

        var user = new User
        {
            FullName = model.FullName,
            Email = model.Email,
            PasswordHash = HashPassword(model.Password),
            CreatedAt = DateTimeHelper.GetVietnamTimeNow(),
            IsActive = true
        };

        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var userRole = new UserRole
        {
            UserId = user.UserId,
            RoleId = studentRole.RoleId
        };

        await _context.UserRoles.AddAsync(userRole);
        await _context.SaveChangesAsync();

        // Tự động đăng nhập sau khi đăng ký thành công
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim("UserId", user.UserId.ToString()),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, "Student")
        };

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity));

        return RedirectToAction("Index", "Courses", new { area = "Student" });
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult AccessDenied(string? returnUrl = null)
    {
        // Chuyển hướng về trang chủ tương ứng với vai trò nếu đã xác thực thành công
        if (User.Identity?.IsAuthenticated == true)
        {
            // Xác định đường dẫn điều hướng dựa trên vai trò người dùng
            string redirectUrl;
            if (User.IsInRole("Admin"))
                redirectUrl = Url.Action("Index", "Dashboard", new { area = "Admin" }) ?? "/";
            else if (User.IsInRole("Instructor"))
                redirectUrl = Url.Action("Index", "Dashboard", new { area = "Instructor" }) ?? "/";
            else
                redirectUrl = Url.Action("Index", "Courses", new { area = "Student" }) ?? "/";
            
            return Redirect(redirectUrl);
        }
        
        // Yêu cầu đăng nhập nếu chưa được xác thực
        return RedirectToAction(nameof(Login), new { returnUrl });
    }

    [HttpGet]
    public IActionResult ForgotPassword()
    {
        return View(new ForgotPasswordViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email && u.IsActive);
        if (user == null)
        {
            // Thông báo chung chung để tránh rò rỉ thông tin người dùng tồn tại trong hệ thống
            TempData["Message"] = "Nếu email tồn tại trong hệ thống, bạn sẽ nhận được hướng dẫn đặt lại mật khẩu.";
            return RedirectToAction(nameof(ForgotPasswordConfirmation));
        }

        // Khởi tạo mã đặt lại mật khẩu (Token định danh tạm thời)
        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        user.ResetToken = token;
        user.ResetTokenExpiry = DateTimeHelper.GetVietnamTimeNow().AddHours(1);
        await _context.SaveChangesAsync();

        // Hiển thị mã trực tiếp trong bản demo để thuận tiện cho việc kiểm thử (không khuyến nghị trong thực tế)
        TempData["ResetToken"] = token;
        TempData["ResetEmail"] = model.Email;

        return RedirectToAction(nameof(ForgotPasswordConfirmation));
    }

    [HttpGet]
    public IActionResult ForgotPasswordConfirmation()
    {
        return View();
    }

    [HttpGet]
    public IActionResult ResetPassword(string? token, string? email)
    {
        if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
        {
            return RedirectToAction(nameof(Login));
        }

        return View(new ResetPasswordViewModel { Token = token, Email = email });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _context.Users.FirstOrDefaultAsync(u => 
            u.Email == model.Email && 
            u.ResetToken == model.Token && 
            u.ResetTokenExpiry > DateTime.UtcNow &&
            u.IsActive);

        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "Token không hợp lệ hoặc đã hết hạn.");
            return View(model);
        }

        user.PasswordHash = HashPassword(model.NewPassword);
        user.ResetToken = null;
        user.ResetTokenExpiry = null;
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Mật khẩu đã được đặt lại thành công. Vui lòng đăng nhập.";
        return RedirectToAction(nameof(Login));
    }

    private IActionResult RedirectToRoleDashboard(string? role = null)
    {
        if (role == null && User.Identity?.IsAuthenticated == true)
        {
            if (User.IsInRole("Admin"))
                role = "Admin";
            else if (User.IsInRole("Instructor"))
                role = "Instructor";
            else
                role = "Student";
        }

        return role switch
        {
            "Admin" => RedirectToAction("Index", "Dashboard", new { area = "Admin" }),
            "Instructor" => RedirectToAction("Index", "Dashboard", new { area = "Instructor" }),
            _ => RedirectToAction("Index", "Courses", new { area = "Student" })
        };
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
