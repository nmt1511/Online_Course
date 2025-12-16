using Online_Course.Models;

namespace Online_Course.Services;

public interface ICourseService
{
    Task<IEnumerable<Course>> GetAllCoursesAsync();
    Task<IEnumerable<Course>> GetCoursesByInstructorAsync(int instructorId);
    Task<IEnumerable<Course>> GetCoursesByCategoryAsync(string category);
    Task<Course?> GetCourseByIdAsync(int id);
    Task<Course> CreateCourseAsync(Course course);
    Task UpdateCourseAsync(Course course);
    Task DeleteCourseAsync(int id);
    Task<int> GetTotalCoursesCountAsync();
    Task<int> GetPublishedCoursesCountAsync();
    Task<IEnumerable<string>> GetAllCategoriesAsync();
}
