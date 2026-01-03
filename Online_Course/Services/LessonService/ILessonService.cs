using Online_Course.Models;

namespace Online_Course.Services.LessonService;

public interface ILessonService
{
    Task<IEnumerable<Lesson>> GetLessonsByCourseAsync(int courseId);
    Task<Lesson?> GetLessonByIdAsync(int id);
    Task<Lesson> CreateLessonAsync(Lesson lesson);
    Task UpdateLessonAsync(Lesson lesson);
    Task DeleteLessonAsync(int id);
    Task ReorderLessonsAsync(int courseId, int[] lessonIds);
    Task<int> GetNextOrderIndexAsync(int courseId);
}
