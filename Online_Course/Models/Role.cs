namespace Online_Course.Models;

public class Role
{
    public int RoleId { get; set; }
    public string Name { get; set; } = string.Empty; // Admin, Instructor, Student
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
