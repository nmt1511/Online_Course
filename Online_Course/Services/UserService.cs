using Microsoft.EntityFrameworkCore;
using Online_Course.Data;
using Online_Course.Models;
using System.Security.Cryptography;
using System.Text;

namespace Online_Course.Services;

public class UserService : IUserService
{
    private readonly ApplicationDbContext _context;

    public UserService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<User>> GetAllUsersAsync()
    {
        // Exclude users with Admin role from the list
        // Loại bỏ các user có vai trò Admin khỏi danh sách
        return await _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .Where(u => !u.UserRoles.Any(ur => ur.Role != null && ur.Role.Name == "Admin"))
            .OrderByDescending(u => u.CreatedAt)
            .ToListAsync();
    }

    public async Task<User?> GetUserByIdAsync(int id)
    {
        return await _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.UserId == id);
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User> CreateUserAsync(User user, string password, int roleId)
    {
        user.PasswordHash = HashPassword(password);
        user.CreatedAt = DateTime.UtcNow;
        user.IsActive = true;

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Assign role
        var userRole = new UserRole
        {
            UserId = user.UserId,
            RoleId = roleId
        };
        _context.UserRoles.Add(userRole);
        await _context.SaveChangesAsync();

        return user;
    }


    public async Task UpdateUserAsync(User user)
    {
        var existingUser = await _context.Users.FindAsync(user.UserId);
        if (existingUser == null)
            throw new ArgumentException("User not found");

        existingUser.FullName = user.FullName;
        existingUser.Email = user.Email;
        existingUser.IsActive = user.IsActive;

        await _context.SaveChangesAsync();
    }

    public async Task DeleteUserAsync(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user != null)
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }
    }

    public async Task AssignRoleAsync(int userId, int roleId)
    {
        // Remove existing roles
        var existingRoles = await _context.UserRoles
            .Where(ur => ur.UserId == userId)
            .ToListAsync();
        _context.UserRoles.RemoveRange(existingRoles);

        // Add new role
        var userRole = new UserRole
        {
            UserId = userId,
            RoleId = roleId
        };
        _context.UserRoles.Add(userRole);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Role>> GetAllRolesAsync()
    {
        return await _context.Roles.ToListAsync();
    }

    public async Task<int> GetTotalUsersCountAsync()
    {
        // Count only non-Admin users
        // Chỉ đếm các user không phải Admin
        return await _context.Users
            .Where(u => !u.UserRoles.Any(ur => ur.Role != null && ur.Role.Name == "Admin"))
            .CountAsync();
    }

    public async Task<int> GetUserCountByRoleAsync(string roleName)
    {
        return await _context.Users
            .Where(u => u.UserRoles.Any(ur => ur.Role.Name == roleName))
            .CountAsync();
    }

    public async Task<IEnumerable<Course>> GetCoursesByUserAsync(int userId)
    {
        return await _context.Courses
            .Include(c => c.CategoryEntity)
            .Include(c => c.Lessons)
            .Include(c => c.Enrollments)
            .Where(c => c.CreatedBy == userId)
            .OrderByDescending(c => c.CourseId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Enrollment>> GetStudentEnrollmentsAsync(int studentId)
    {
        // Lấy danh sách đăng ký học của sinh viên kèm thông tin khóa học và bài học
        return await _context.Enrollments
            .Include(e => e.Course)
                .ThenInclude(c => c.Lessons)
            .Include(e => e.Course)
                .ThenInclude(c => c.CategoryEntity)
            .Where(e => e.StudentId == studentId)
            .OrderByDescending(e => e.EnrolledAt)
            .ToListAsync();
    }

    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }
}
