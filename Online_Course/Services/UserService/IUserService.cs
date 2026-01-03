using Online_Course.Models;

namespace Online_Course.Services.UserService;

public interface IUserService
{
    Task<IEnumerable<User>> GetAllUsersAsync();
    Task<User?> GetUserByIdAsync(int id);
    Task<User?> GetUserByEmailAsync(string email);
    Task<User> CreateUserAsync(User user, string password, int roleId);
    Task UpdateUserAsync(User user);
    Task DeleteUserAsync(int id);
    Task AssignRoleAsync(int userId, int roleId);
    Task<IEnumerable<Role>> GetAllRolesAsync();
    Task<int> GetTotalUsersCountAsync();
    Task<int> GetUserCountByRoleAsync(string roleName);
    Task<IEnumerable<Course>> GetCoursesByUserAsync(int userId);
    Task<IEnumerable<Enrollment>> GetStudentEnrollmentsAsync(int studentId);
}
