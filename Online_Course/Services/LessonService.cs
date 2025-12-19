using Microsoft.EntityFrameworkCore;
using Online_Course.Data;
using Online_Course.Helper;
using Online_Course.Models;

namespace Online_Course.Services;

public class LessonService : ILessonService
{
    private readonly ApplicationDbContext _context;

    public LessonService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Lesson>> GetLessonsByCourseAsync(int courseId)
    {
        return await _context.Lessons
            .Where(l => l.CourseId == courseId)
            .OrderBy(l => l.OrderIndex)
            .ToListAsync();
    }

    public async Task<Lesson?> GetLessonByIdAsync(int id)
    {
        return await _context.Lessons
            .Include(l => l.Course)
            .FirstOrDefaultAsync(l => l.LessonId == id);
    }

    public async Task<Lesson> CreateLessonAsync(Lesson lesson)
    {
        // Tự động tính OrderIndex nếu chưa được set
        if (lesson.OrderIndex == 0)
        {
            lesson.OrderIndex = await GetNextOrderIndexAsync(lesson.CourseId);
        }
        
        // Set thời gian tạo
        lesson.CreatedAt = DateTimeHelper.GetVietnamTimeNow();

        _context.Lessons.Add(lesson);
        await _context.SaveChangesAsync();
        return lesson;
    }

    public async Task UpdateLessonAsync(Lesson lesson)
    {
        var existingLesson = await _context.Lessons.FindAsync(lesson.LessonId);
        if (existingLesson == null)
            throw new ArgumentException("Lesson not found");

        // Cập nhật các trường cơ bản
        existingLesson.Title = lesson.Title;
        existingLesson.Description = lesson.Description;
        existingLesson.ContentUrl = lesson.ContentUrl;
        existingLesson.OrderIndex = lesson.OrderIndex;
        
        // Cập nhật các trường mới cho PDF và Video
        existingLesson.LessonType = lesson.LessonType;
        existingLesson.TotalPages = lesson.TotalPages;
        existingLesson.TotalDurationSeconds = lesson.TotalDurationSeconds;
        existingLesson.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    public async Task DeleteLessonAsync(int id)
    {
        var lesson = await _context.Lessons.FindAsync(id);
        if (lesson != null)
        {
            _context.Lessons.Remove(lesson);
            await _context.SaveChangesAsync();
        }
    }

    public async Task ReorderLessonsAsync(int courseId, int[] lessonIds)
    {
        var lessons = await _context.Lessons
            .Where(l => l.CourseId == courseId)
            .ToListAsync();

        for (int i = 0; i < lessonIds.Length; i++)
        {
            var lesson = lessons.FirstOrDefault(l => l.LessonId == lessonIds[i]);
            if (lesson != null)
            {
                lesson.OrderIndex = i + 1;
            }
        }

        await _context.SaveChangesAsync();
    }

    public async Task<int> GetNextOrderIndexAsync(int courseId)
    {
        var maxOrder = await _context.Lessons
            .Where(l => l.CourseId == courseId)
            .MaxAsync(l => (int?)l.OrderIndex) ?? 0;
        
        return maxOrder + 1;
    }
}
