using Online_Course.Helper;

namespace Online_Course.Models;

public class User
{
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTimeHelper.GetVietnamTimeNow();
    public bool IsActive { get; set; } = true;
    public string? ResetToken { get; set; }
    public DateTime? ResetTokenExpiry { get; set; }
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<Course> CreatedCourses { get; set; } = new List<Course>();
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    public ICollection<Progress> Progresses { get; set; } = new List<Progress>();
}
