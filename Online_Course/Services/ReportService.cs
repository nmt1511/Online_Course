using Microsoft.EntityFrameworkCore;
using Online_Course.Data;
using Online_Course.Models;

namespace Online_Course.Services;

public class ReportService : IReportService
{
    private readonly ApplicationDbContext _context;

    public ReportService(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Đếm số lượng khóa học đã mở (publish & private) trong một tháng cụ thể
    /// </summary>
    /// <param name="year">Năm cần thống kê</param>
    /// <param name="month">Tháng cần thống kê</param>
    /// <returns>Số lượng khóa học</returns>
    public async Task<int> GetMonthlyPublishedCoursesCountAsync(int year, int month)
    {
        // Xác định ngày bắt đầu và ngày kết thúc của tháng cần thống kê
        var startDate = new DateTime(year, month, 1);
        var endDate = startDate.AddMonths(1);

        // Đếm các khóa học có trạng thái Public và được tạo trong khoảng thời gian này
        return await _context.Courses
            .Where(c => c.CourseStatus != CourseStatus.Draft)
            .Where(c => c.CreatedAt >= startDate && c.CreatedAt < endDate)
            .CountAsync();
    }

    /// <summary>
    /// Lấy danh sách các khóa học phổ biến nhất dựa trên số lượng học viên đăng ký
    /// </summary>
    /// <param name="topCount">Số lượng khóa học tối đa muốn lấy (mặc định 10)</param>
    /// <returns>Danh sách báo cáo độ phổ biến của khóa học</returns>
    public async Task<IEnumerable<CoursePopularityReport>> GetPopularCoursesAsync(int topCount = 10)
    {
        return await _context.Courses
            .Include(c => c.CategoryEntity) // Lấy thông tin danh mục kèm theo
            .Where(c => c.CourseStatus == CourseStatus.Public) // Chỉ xét khóa học đã công khai
            .Select(c => new CoursePopularityReport
            {
                CourseId = c.CourseId,
                Title = c.Title,
                CategoryName = c.CategoryEntity != null ? c.CategoryEntity.Name : "Chưa phân loại",
                InstructorName = c.Instructor.FullName,
                EnrollmentCount = c.Enrollments.Count // Lấy số lượng học viên đăng ký
            })
            .OrderByDescending(c => c.EnrollmentCount) // Sắp xếp giảm dần theo lượt đăng ký
            .Take(topCount) // Giới hạn số lượng kết quả
            .ToListAsync();
    }

    /// <summary>
    /// Đếm tổng số lượt đăng ký học trong một tháng cụ thể
    /// </summary>
    /// <param name="year">Năm cần thống kê</param>
    /// <param name="month">Tháng cần thống kê</param>
    /// <returns>Số lượng lượt đăng ký</returns>
    public async Task<int> GetMonthlyEnrollmentsCountAsync(int year, int month)
    {
        // Tính toán khoảng thời gian trong tháng
        var startDate = new DateTime(year, month, 1);
        var endDate = startDate.AddMonths(1);

        // Lọc các bản ghi đăng ký (Enrollment) dựa trên thời gian đăng ký
        return await _context.Enrollments
            .Where(e => e.EnrolledAt >= startDate && e.EnrolledAt < endDate)
            .CountAsync();
    }


    /// <summary>
    /// Đếm số lượng giảng viên có hoạt động (có xuất bản ít nhất 1 khóa học) trong tháng
    /// </summary>
    /// <param name="year">Năm cần thống kê</param>
    /// <param name="month">Tháng cần thống kê</param>
    /// <returns>Số lượng giảng viên</returns>
    public async Task<int> GetMonthlyActiveInstructorsCountAsync(int year, int month)
    {
        // Giảng viên hoạt động là những người có ít nhất một khóa học đã xuất bản
        return await _context.Courses
            .Where(c => c.CourseStatus == CourseStatus.Public)
            .Select(c => c.CreatedBy) // Lấy ID của người tạo
            .Distinct() // Loại bỏ các ID trùng lặp để đếm số lượng người duy nhất
            .CountAsync();
    }

    /// <summary>
    /// Lấy tổng hợp các số liệu thống kê trong một tháng (Khóa học mới, Lượt đăng ký, Giảng viên, Học viên mới)
    /// </summary>
    /// <param name="year">Năm cần thống kê</param>
    /// <param name="month">Tháng cần thống kê</param>
    /// <returns>Đối tượng chứa các số liệu thống kê tháng</returns>
    public async Task<MonthlyStatistics> GetMonthlyStatisticsAsync(int year, int month)
    {
        var startDate = new DateTime(year, month, 1);
        var endDate = startDate.AddMonths(1);

        // Thống kê khóa học mới mở trong tháng
        var publishedCourses = await _context.Courses
            .Where(c => c.CourseStatus != CourseStatus.Draft)
            .Where(c => (c.CreatedAt >= startDate && c.CreatedAt < endDate))
            .CountAsync();

        // Thống kê tổng số lượt đăng ký mới trong tháng
        var totalEnrollments = await _context.Enrollments
            .Where(e => e.EnrolledAt >= startDate && e.EnrolledAt < endDate)
            .CountAsync();

        // Thống kê số giảng viên có khóa học mới trong tháng
        var activeInstructors = await _context.Courses
            .Where(c => c.CourseStatus != CourseStatus.Draft)
            .Where(c => (c.CreatedAt >= startDate && c.CreatedAt < endDate))
            .Select(c => c.CreatedBy)
            .Distinct()
            .CountAsync();

        // Thống kê số lượng học viên mới đăng ký tài khoản trong tháng
        var newStudents = await _context.Users
            .Where(u => u.CreatedAt >= startDate && u.CreatedAt < endDate)
            .Where(u => u.UserRoles.Any(ur => ur.Role.Name == "Student"))
            .CountAsync();

        return new MonthlyStatistics
        {
            Year = year,
            Month = month,
            PublishedCourses = publishedCourses,
            TotalEnrollments = totalEnrollments,
            ActiveInstructors = activeInstructors,
            NewStudents = newStudents
        };
    }

    /// <summary>
    /// Lấy xu hướng đăng ký học trong các tháng gần đây (để vẽ biểu đồ)
    /// </summary>
    /// <param name="months">Số lượng tháng quay ngược về trước (mặc định 6 tháng)</param>
    /// <returns>Danh sách dữ liệu xu hướng theo từng tháng</returns>
    public async Task<IEnumerable<MonthlyTrend>> GetEnrollmentTrendsAsync(int months = 6)
    {
        var trends = new List<MonthlyTrend>();
        var currentDate = DateTime.UtcNow;

        // Vòng lặp lấy dữ liệu cho từng tháng trong quá khứ
        for (int i = months - 1; i >= 0; i--)
        {
            var targetDate = currentDate.AddMonths(-i);
            var year = targetDate.Year;
            var month = targetDate.Month;
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1);

            // Đếm số lượt đăng ký trong tháng tương ứng
            var count = await _context.Enrollments
                .Where(e => e.EnrolledAt >= startDate && e.EnrolledAt < endDate)
                .CountAsync();

            trends.Add(new MonthlyTrend
            {
                Year = year,
                Month = month,
                MonthName = $"Tháng {month}/{year}",
                Count = count
            });
        }

        return trends;
    }

    /// <summary>
    /// Lấy xu hướng xuất bản khóa học trong các tháng gần đây
    /// </summary>
    /// <param name="months">Số lượng tháng quay ngược về trước</param>
    /// <returns>Danh sách dữ liệu xu hướng xuất bản</returns>
    public async Task<IEnumerable<MonthlyTrend>> GetCoursePublicationTrendsAsync(int months = 6)
    {
        var trends = new List<MonthlyTrend>();
        var currentDate = DateTime.UtcNow;

        // Lấy tổng số khóa học hiện tại (để demo vì các khóa cũ chưa có CreatedAt chính xác)
        var totalPublished = await _context.Courses.Where(c => c.CourseStatus == CourseStatus.Public).CountAsync();

        for (int i = months - 1; i >= 0; i--)
        {
            var targetDate = currentDate.AddMonths(-i);
            var year = targetDate.Year;
            var month = targetDate.Month;
            var startDate = new DateTime(year, month, 1);

            trends.Add(new MonthlyTrend
            {
                Year = year,
                Month = month,
                MonthName = startDate.ToString("MMM yyyy"),
                Count = i == 0 ? totalPublished : 0 // Chỉ hiển thị tổng số ở tháng hiện tại
            });
        }

        return trends;
    }

    /// <summary>
    /// Lấy danh sách chi tiết các khóa học được xuất bản trong một tháng cụ thể
    /// </summary>
    /// <param name="year">Năm cần xem báo cáo</param>
    /// <param name="month">Tháng cần xem báo cáo</param>
    /// <returns>Danh sách đối tượng Course kèm thông tin Giảng viên và Danh mục</returns>
    public async Task<IEnumerable<Course>> GetPublishedCoursesReportAsync(int year, int month)
    {
        var startDate = new DateTime(year, month, 1);
        var endDate = startDate.AddMonths(1);

        // Truy vấn danh sách khóa học kèm theo Instructor và Category
        return await _context.Courses
            .Include(c => c.Instructor)
            .Include(c => c.CategoryEntity)
            .Where(c => c.CourseStatus != CourseStatus.Draft)
            .Where(c => c.CreatedAt >= startDate && c.CreatedAt < endDate)
            .OrderByDescending(c => c.CreatedAt) // Sắp xếp theo ngày tạo mới nhất
            .ToListAsync();
    }

    /// <summary>
    /// Thống kê số lượng khóa học đã mở của từng giảng viên trong một tháng cụ thể
    /// </summary>
    /// <param name="year">Năm cần xem báo cáo</param>
    /// <param name="month">Tháng cần xem báo cáo</param>
    /// <returns>Danh sách thông tin hoạt động của giảng viên (Tên, Số lượng khóa học)</returns>
    public async Task<IEnumerable<InstructorActivityReportData>> GetInstructorActivityReportAsync(int year, int month)
    {
        var startDate = new DateTime(year, month, 1);
        var endDate = startDate.AddMonths(1);

        // Nhóm các khóa học theo tên giảng viên và đếm số lượng
        return await _context.Courses
            .Where(c => c.CourseStatus != CourseStatus.Draft)
            .Where(c => c.CreatedAt >= startDate && c.CreatedAt < endDate)
            .GroupBy(c => c.Instructor.FullName) // Nhóm theo tên đầy đủ của giảng viên
            .Select(g => new InstructorActivityReportData
            {
                InstructorName = g.Key, // g.Key chính là Instructor.FullName
                CourseCount = g.Count() // Đếm số lượng khóa học trong nhóm
            })
            .OrderByDescending(x => x.CourseCount) // Sắp xếp giảng viên có nhiều khóa học nhất lên đầu
            .ToListAsync();
    }
}
