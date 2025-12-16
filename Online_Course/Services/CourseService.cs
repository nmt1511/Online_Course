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
            .OrderByDescending(c => c.CourseId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Course>> GetCoursesByInstructorAsync(int instructorId)
    {
        return await _context.Courses
            .Include(c => c.Instructor)
            .Include(c => c.Enrollments)
            .Where(c => c.CreatedBy == instructorId)
            .OrderByDescending(c => c.CourseId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Course>> GetCoursesByCategoryAsync(string category)
    {
        return await _context.Courses
            .Include(c => c.Instructor)
            .Include(c => c.Enrollments)
            .Where(c => c.Category == category)
            .OrderByDescending(c => c.CourseId)
            .ToListAsync();
    }

    public async Task<Course?> GetCourseByIdAsync(int id)
    {
        return await _context.Courses
            .Include(c => c.Instructor)
            .Include(c => c.Lessons)
            .Include(c => c.Enrollments)
            .FirstOrDefaultAsync(c => c.CourseId == id);
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
        existingCourse.Category = course.Category;
        existingCourse.ThumbnailUrl = course.ThumbnailUrl;
        existingCourse.CreatedBy = course.CreatedBy;
        existingCourse.Status = course.Status;
        existingCourse.CategoryId = course.CategoryId;

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
            // Delete all progress records for lessons in this course
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
        return await _context.Courses.CountAsync(c => c.Status == CourseStatus.Public);
    }

    public async Task<IEnumerable<string>> GetAllCategoriesAsync()
    {
        return await _context.Courses
            .Where(c => !string.IsNullOrEmpty(c.Category))
            .Select(c => c.Category)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();
    }
}
