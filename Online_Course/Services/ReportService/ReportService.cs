using Microsoft.EntityFrameworkCore;
using Online_Course.Data;
using Online_Course.Models;

namespace Online_Course.Services.ReportService;

public class ReportService : IReportService
{
    private readonly ApplicationDbContext _context;

    public ReportService(ApplicationDbContext context)
    {
        _context = context;
    }

    // Đếm số lượng khóa học đã mở (Công khai hoặc Riêng tư) trong một tháng cụ thể
    // year: Năm cần thống kê
    // month: Tháng cần thống kê
    // Trả về số lượng khóa học tìm thấy
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

    // Lấy danh sách các khóa học phổ biến nhất dựa trên số lượng học viên đăng ký
    // topCount: Số lượng khóa học tối đa muốn lấy (mặc định là 10)
    // Trả về danh sách báo cáo độ phổ biến của khóa học
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

    // Đếm tổng số lượt đăng ký học trong một tháng cụ thể.
    // year: Năm cần thống kê.
    // month: Tháng cần thống kê.
    // Trả về tổng số lượt đăng ký mới.
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


    // Đếm số lượng giảng viên có hoạt động (đã xuất bản ít nhất 1 khóa học) trong tháng.
    // year: Năm cần thống kê.
    // month: Tháng cần thống kê.
    // Trả về số lượng giảng viên có hoạt động.
    public async Task<int> GetMonthlyActiveInstructorsCountAsync(int year, int month)
    {
        // Giảng viên hoạt động là những người có ít nhất một khóa học đã xuất bản
        return await _context.Courses
            .Where(c => c.CourseStatus == CourseStatus.Public)
            .Select(c => c.CreatedBy) // Lấy ID của người tạo
            .Distinct() // Loại bỏ các ID trùng lặp để đếm số lượng người duy nhất
            .CountAsync();
    }

    // Lấy tổng hợp các số liệu thống kê trong tháng (khóa học mới, lượt đăng ký, giảng viên, học viên mới).
    // year: Năm cần thống kê.
    // month: Tháng cần thống kê.
    // Trả về đối tượng chứa dữ liệu thống kê tháng.
    public async Task<MonthlyStatistics> GetMonthlyStatisticsAsync(int year, int month)
    {
        var startDate = new DateTime(year, month, 1);
        var endDate = startDate.AddMonths(1);

        // Thống kê khóa học mới mở trong tháng
        var publishedCourses = await _context.Courses
            .Where(c => c.CourseStatus != CourseStatus.Draft)
            .Where(c => c.CreatedAt >= startDate && c.CreatedAt < endDate)
            .CountAsync();

        // Thống kê tổng số lượt đăng ký mới trong tháng
        var totalEnrollments = await _context.Enrollments
            .Where(e => e.EnrolledAt >= startDate && e.EnrolledAt < endDate)
            .CountAsync();

        // Thống kê số giảng viên có khóa học mới trong tháng
        var activeInstructors = await _context.Courses
            .Where(c => c.CourseStatus != CourseStatus.Draft)
            .Where(c => c.CreatedAt >= startDate && c.CreatedAt < endDate)
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

    // Lấy xu hướng đăng ký học trong các tháng gần đây nhằm phục vụ việc vẽ biểu đồ.
    // months: Số lượng tháng quay ngược về trước kể từ thời điểm hiện tại (mặc định 6 tháng).
    // Trả về danh sách dữ liệu xu hướng theo từng tháng.
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

    // Lấy xu hướng xuất bản khóa học trong các tháng gần đây.
    // months: Số lượng tháng quay ngược về trước.
    // Trả về danh sách thống kê số lượng khóa học được xuất bản.
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

    // Lấy danh sách chi tiết các khóa học được xuất bản trong một tháng cụ thể
    // year: Năm cần xem báo cáo
    // month: Tháng cần xem báo cáo
    // Trả về danh sách khóa học kèm thông tin giảng viên và danh mục tương ứng
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

    // Thống kê số lượng khóa học đã mở của từng giảng viên trong một tháng cụ thể
    // year: Năm cần xem báo cáo
    // month: Tháng cần xem báo cáo
    // Trả về danh sách thông tin hoạt động của giảng viên (Tên, Số lượng khóa học)
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
