using Microsoft.EntityFrameworkCore;
using Online_Course.Data;
using Online_Course.Models;
using System.Security.Cryptography;
using System.Text;

namespace Online_Course.Services.UserService;

public class UserService : IUserService
{
    private readonly ApplicationDbContext _context;

    public UserService(ApplicationDbContext context)
    {
        _context = context;
    }

    // Lấy danh sách người dùng, loại bỏ các tài khoản có vai trò Admin
    public async Task<IEnumerable<User>> GetAllUsersAsync()
    {
        // Loại bỏ các user có vai trò Admin khỏi danh sách
        return await _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .Where(u => !u.UserRoles.Any(ur => ur.Role != null && ur.Role.Name == "Admin"))
            .OrderByDescending(u => u.CreatedAt)
            .ToListAsync();
    }

    // Tìm kiếm người dùng theo ID
    public async Task<User?> GetUserByIdAsync(int id)
    {
        return await _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.UserId == id);
    }

    // Tìm kiếm người dùng theo địa chỉ Email
    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Email == email);
    }



    // Tạo tài khoản người dùng mới và gán vai trò tương ứng
    public async Task<User> CreateUserAsync(User user, string password, int roleId)
    {
        user.PasswordHash = HashPassword(password);
        user.CreatedAt = DateTime.UtcNow;
        user.IsActive = true;

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Đính kèm vai trò tương ứng cho người dùng mới
        var userRole = new UserRole
        {
            UserId = user.UserId,
            RoleId = roleId
        };
        _context.UserRoles.Add(userRole);
        await _context.SaveChangesAsync();

        return user;
    }


    // Cập nhật thông tin cơ bản của người dùng
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

    // Xóa tài khoản người dùng khỏi hệ thống
    public async Task DeleteUserAsync(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user != null)
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }
    }

    // Gán vai trò mới cho người dùng (thay thế vai trò cũ)
    public async Task AssignRoleAsync(int userId, int roleId)
    {
        // Loại bỏ các vai trò hiện tại của người dùng
        var existingRoles = await _context.UserRoles
            .Where(ur => ur.UserId == userId)
            .ToListAsync();
        _context.UserRoles.RemoveRange(existingRoles);

        // Chỉ định vai trò mới cho người dùng
        var userRole = new UserRole
        {
            UserId = userId,
            RoleId = roleId
        };
        _context.UserRoles.Add(userRole);
        await _context.SaveChangesAsync();
    }

    // Lấy danh sách tất cả các vai trò trong hệ thống
    public async Task<IEnumerable<Role>> GetAllRolesAsync()
    {
        return await _context.Roles.ToListAsync();
    }

    // Thống kê tổng số lượng người dùng (không bao gồm Admin)
    public async Task<int> GetTotalUsersCountAsync()
    {
        // Chỉ thực hiện đếm đối với các tài khoản không phải Admin
        return await _context.Users
            .Where(u => !u.UserRoles.Any(ur => ur.Role != null && ur.Role.Name == "Admin"))
            .CountAsync();
    }

    // Đếm số lượng người dùng theo tên vai trò cụ thể
    public async Task<int> GetUserCountByRoleAsync(string roleName)
    {
        return await _context.Users
            .Where(u => u.UserRoles.Any(ur => ur.Role.Name == roleName))
            .CountAsync();
    }

    // Lấy danh sách các khóa học do người dùng (giảng viên) tạo ra
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

    // Lấy lịch sử và trạng thái đăng ký học tập của sinh viên
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



    // Hàm băm mật khẩu sử dụng thuật toán SHA256
    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }
}
