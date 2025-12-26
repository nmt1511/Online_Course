using Online_Course.Models;

namespace Online_Course.Services;

public interface IEnrollmentService
{
    //Kiểm tra sinh viên có đăng ký khóa học đó không
    Task<bool> IsEnrolledAsync(int studentId, int courseId);

    //Sinh viên đăng ký khóa học
    Task<Enrollment> EnrollAsync(int studentId, int courseId);

    //Sinh viên hủy đăng ký
    Task<bool> UnenrollAsync(int studentId, int courseId);

    //Lấy các khóa học sinh viên đã đăng ký
    Task<IEnumerable<Enrollment>> GetEnrollmentsByStudentAsync(int studentId);

    //Lấy ds sinh viên đăng ký khóa học
    Task<IEnumerable<Enrollment>> GetEnrollmentsByCourseAsync(int courseId);
    Task<int> GetEnrollmentCountByCourseAsync(int courseId);

    //Lưu học viên được chỉ định vào khóa học
    Task EnrollStudentAsync(Enrollment enrollment);
}
