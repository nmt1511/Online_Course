using Microsoft.EntityFrameworkCore;
using Online_Course.Data;
using Online_Course.Models;

namespace Online_Course.Services;

public class CourseService : ICourseService
{
    private readonly ApplicationDbContext _context;

    public CourseService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Course>> GetAllCoursesAsync()
    {
        return await _context.Courses
            .Include(c => c.Instructor)
            .Include(c => c.Enrollments)
            .Include(c => c.CategoryEntity)
            .Include(c => c.Lessons)
            .OrderByDescending(c => c.CourseId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Course>> GetAllCoursesPublicAsync()
    {
        return await _context.Courses
            .Where(c => c.CourseStatus == CourseStatus.Public)
            .Include(c => c.Instructor)
            .Include(c => c.Enrollments)
            .Include(c => c.CategoryEntity)
            .Include(c => c.Lessons)
            .OrderByDescending(c => c.CourseId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Course>> GetCoursesByInstructorAsync(int instructorId)
    {
        return await _context.Courses
            .Include(c => c.Instructor)
            .Include(c => c.Enrollments)
            .Include(c => c.Lessons)
            .Include(c => c.CategoryEntity)
            .Where(c => c.CreatedBy == instructorId)
            .OrderByDescending(c => c.CourseId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Course>> GetCoursesByCategoryAsync(int categoryId)
    {
        return await _context.Courses
            .Include(c => c.Instructor)
            .Include(c => c.Enrollments)
            .Include(c => c.CategoryEntity)
            .Where(c => c.CategoryId == categoryId)
            .OrderByDescending(c => c.CourseId)
            .ToListAsync();
    }

    public async Task<Course?> GetCourseByIdAsync(int id)
    {
        var course = await _context.Courses
            .Include(c => c.Instructor)
            .Include(c => c.Lessons)
            .Include(c => c.Enrollments)
            .Include(c => c.CategoryEntity)
            .FirstOrDefaultAsync(c => c.CourseId == id);

        // Logic tự động đóng khóa học nếu là loại 'Fixed_Time' và đã hết thời gian học
        if (course != null && course.CourseType == CourseType.Fixed_Time && course.EndDate.HasValue)
        {
            // So sánh ngày kết thúc với ngày hiện tại (theo giờ VN)
            if (DateTime.Now.Date > course.EndDate.Value.Date && course.CourseStatus != CourseStatus.Closed)
            {
                course.CourseStatus = CourseStatus.Closed;
                await _context.SaveChangesAsync(); // Cập nhật trạng thái mới vào cơ sở dữ liệu
            }
        }

        return course;
    }


    public async Task<Course> CreateCourseAsync(Course course)
    {
        _context.Courses.Add(course);
        await _context.SaveChangesAsync();
        return course;
    }

    public async Task UpdateCourseAsync(Course course)
    {
        var existingCourse = await _context.Courses.FindAsync(course.CourseId);
        if (existingCourse == null)
            throw new ArgumentException("Course not found");

        existingCourse.Title = course.Title;
        existingCourse.Description = course.Description;
        existingCourse.ThumbnailUrl = course.ThumbnailUrl;
        existingCourse.CreatedBy = course.CreatedBy;
        existingCourse.CourseStatus = course.CourseStatus;
        existingCourse.CategoryId = course.CategoryId;
        
        // Cập nhật thêm các trường về loại và thời gian khóa học
        existingCourse.CourseType = course.CourseType;
        existingCourse.RegistrationStartDate = course.RegistrationStartDate;
        existingCourse.RegistrationEndDate = course.RegistrationEndDate;
        existingCourse.StartDate = course.StartDate;
        existingCourse.EndDate = course.EndDate;

        await _context.SaveChangesAsync();
    }

    public async Task DeleteCourseAsync(int id)
    {
        var course = await _context.Courses
            .Include(c => c.Lessons)
            .Include(c => c.Enrollments)
            .FirstOrDefaultAsync(c => c.CourseId == id);
            
        if (course != null)
        {
            // Kiểm tra bảo mật tầng Service: Không cho xóa nếu có học viên
            if (course.Enrollments.Any())
            {
                throw new InvalidOperationException("Không thể xóa khóa học đã có học viên đăng ký.");
            }

            // Xóa tất cả tiến độ học tập liên quan
            var lessonIds = course.Lessons.Select(l => l.LessonId).ToList();
            var progressRecords = await _context.Progresses
                .Where(p => lessonIds.Contains(p.LessonId))
                .ToListAsync();
            _context.Progresses.RemoveRange(progressRecords);

            // Delete all enrollments for this course
            _context.Enrollments.RemoveRange(course.Enrollments);

            // Delete all lessons
            _context.Lessons.RemoveRange(course.Lessons);

            // Delete the course
            _context.Courses.Remove(course);
            
            await _context.SaveChangesAsync();
        }
    }

    public async Task<int> GetTotalCoursesCountAsync()
    {
        return await _context.Courses.CountAsync();
    }

    public async Task<int> GetPublishedCoursesCountAsync()
    {
        return await _context.Courses.CountAsync(c => c.CourseStatus == CourseStatus.Public);
    }

    public async Task<int> GetDraftCoursesCountAsync()
    {
        return await _context.Courses.CountAsync(c => c.CourseStatus == CourseStatus.Draft);
    }

    public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
    {
        return await _context.Categories
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }
}
