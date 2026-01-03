using Microsoft.EntityFrameworkCore;
using Online_Course.Data;
using Online_Course.Models;

namespace Online_Course.Services.EnrollmentService;

public class EnrollmentService : IEnrollmentService
{
    private readonly ApplicationDbContext _context;

    public EnrollmentService(ApplicationDbContext context)
    {
        _context = context;
    }

    // Kiểm tra xem sinh viên đã đăng ký khóa học này chưa
    public async Task<bool> IsEnrolledAsync(int studentId, int courseId)
    {
        return await _context.Enrollments
            .AnyAsync(e => e.StudentId == studentId && e.CourseId == courseId);
    }

    // Đăng ký khóa học mới cho sinh viên
    public async Task<Enrollment> EnrollAsync(int studentId, int courseId)
    {
        var enrollment = new Enrollment
        {
            StudentId = studentId,
            CourseId = courseId,
            EnrolledAt = DateTime.UtcNow
        };

        _context.Enrollments.Add(enrollment);
        await _context.SaveChangesAsync();
        return enrollment;
    }

    // Hủy đăng ký khóa học và xóa toàn bộ tiến độ học tập liên quan
    public async Task<bool> UnenrollAsync(int studentId, int courseId)
    {
        var enrollment = await _context.Enrollments
            .FirstOrDefaultAsync(e => e.StudentId == studentId && e.CourseId == courseId);

        if (enrollment == null)
        {
            return false;
        }

        // Đồng thời xóa toàn bộ bản ghi tiến độ học tập liên quan
        var progressRecords = await _context.Progresses
            .Where(p => p.StudentId == studentId && p.Lesson.CourseId == courseId)
            .ToListAsync();

        _context.Progresses.RemoveRange(progressRecords);
        _context.Enrollments.Remove(enrollment);
        await _context.SaveChangesAsync();
        return true;
    }

    // Lấy danh sách các khóa học mà sinh viên đã đăng ký
    public async Task<IEnumerable<Enrollment>> GetEnrollmentsByStudentAsync(int studentId)
    {
        return await _context.Enrollments
            .Include(e => e.Course)
                .ThenInclude(c => c.Lessons)
            .Include(e => e.Course)
                .ThenInclude(c => c.CategoryEntity)
            .Include(e => e.Student)
            .Where(e => e.StudentId == studentId)
            .OrderByDescending(e => e.EnrolledAt)
            .ToListAsync();
    }

    // Lấy danh sách sinh viên đã đăng ký một khóa học cụ thể
    public async Task<IEnumerable<Enrollment>> GetEnrollmentsByCourseAsync(int courseId)
    {
        return await _context.Enrollments
            .Include(e => e.Student)
            .Include(e => e.Course)
                .ThenInclude(c => c.Lessons)
            .Where(e => e.CourseId == courseId)
            .OrderByDescending(e => e.EnrolledAt)
            .ToListAsync();
    }

    // Đếm tổng số học viên của một khóa học
    public async Task<int> GetEnrollmentCountByCourseAsync(int courseId)
    {
        return await _context.Enrollments
            .CountAsync(e => e.CourseId == courseId);
    }

    // Lưu thông tin đăng ký của học viên vào cơ sở dữ liệu
    public async Task EnrollStudentAsync(Enrollment enrollment)
    {
        _context.Enrollments.Add(enrollment);
        await _context.SaveChangesAsync();
    }
}
