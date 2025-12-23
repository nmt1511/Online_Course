using Microsoft.EntityFrameworkCore;
using Online_Course.Data;
using Online_Course.Models;

namespace Online_Course.Services;

public class ProgressService : IProgressService
{
    private readonly ApplicationDbContext _context;

    public ProgressService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Progress> MarkLessonCompleteAsync(int studentId, int lessonId)
    {
        var existingProgress = await _context.Progresses
            .FirstOrDefaultAsync(p => p.StudentId == studentId && p.LessonId == lessonId);

        if (existingProgress != null)
        {
            existingProgress.IsCompleted = true;
            existingProgress.LastUpdate = DateTime.UtcNow;
        }
        else
        {
            existingProgress = new Progress
            {
                StudentId = studentId,
                LessonId = lessonId,
                IsCompleted = true,
                LastUpdate = DateTime.UtcNow
            };
            _context.Progresses.Add(existingProgress);
        }

        await _context.SaveChangesAsync();
        return existingProgress;
    }

    public async Task<IEnumerable<Progress>> GetProgressByStudentAndCourseAsync(int studentId, int courseId)
    {
        return await _context.Progresses
            .Include(p => p.Lesson)
            .Where(p => p.StudentId == studentId && p.Lesson.CourseId == courseId)
            .ToListAsync();
    }

    //Đếm số lượng bài học hoàn thành của 1 khóa học
    public async Task<int> GetCompletedLessonsCountAsync(int studentId, int courseId)
    {
        return await _context.Progresses
            .Include(p => p.Lesson)
            .CountAsync(p => p.StudentId == studentId 
                && p.Lesson.CourseId == courseId 
                && p.IsCompleted);
    }

    //Tính % hoàn thành khóa học
    public async Task<double> CalculateProgressPercentageAsync(int studentId, int courseId)
    {
        var totalLessons = await _context.Lessons
            .CountAsync(l => l.CourseId == courseId);

        if (totalLessons == 0)
            return 0;

        var completedLessons = await GetCompletedLessonsCountAsync(studentId, courseId);
        return (double)completedLessons / totalLessons * 100;
    }

    public async Task<bool> IsLessonCompletedAsync(int studentId, int lessonId)
    {
        return await _context.Progresses
            .AnyAsync(p => p.StudentId == studentId && p.LessonId == lessonId && p.IsCompleted);
    }
}
