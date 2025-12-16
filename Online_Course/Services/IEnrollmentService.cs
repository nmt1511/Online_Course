using Online_Course.Models;

namespace Online_Course.Services;

public interface IEnrollmentService
{
    Task<bool> IsEnrolledAsync(int studentId, int courseId);
    Task<Enrollment> EnrollAsync(int studentId, int courseId);
    Task<bool> UnenrollAsync(int studentId, int courseId);
    Task<IEnumerable<Enrollment>> GetEnrollmentsByStudentAsync(int studentId);
    Task<IEnumerable<Enrollment>> GetEnrollmentsByCourseAsync(int courseId);
    Task<int> GetEnrollmentCountByCourseAsync(int courseId);
}
