using Online_Course.Models;

namespace Online_Course.Services.CourseService;

public interface ICourseService
{
    Task<IEnumerable<Course>> GetAllCoursesAsync();
    //lấy danh sách khóa học công khai cho học viên
    Task<IEnumerable<Course>> GetAllCoursesPublicAsync();
    Task<IEnumerable<Course>> GetCoursesByInstructorAsync(int instructorId);
    Task<IEnumerable<Course>> GetCoursesByCategoryAsync(int categoryId);
    Task<Course?> GetCourseByIdAsync(int id);
    Task<Course> CreateCourseAsync(Course course);
    Task UpdateCourseAsync(Course course);
    Task DeleteCourseAsync(int id);
    Task<int> GetTotalCoursesCountAsync();
    Task<int> GetPublishedCoursesCountAsync();
    Task<int> GetDraftCoursesCountAsync();
    Task<IEnumerable<Category>> GetAllCategoriesAsync();
}
