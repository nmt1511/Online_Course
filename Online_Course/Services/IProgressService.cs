using Online_Course.Models;

namespace Online_Course.Services;

public interface IProgressService
{
    Task<Progress> MarkLessonCompleteAsync(int studentId, int lessonId);
    Task<IEnumerable<Progress>> GetProgressByStudentAndCourseAsync(int studentId, int courseId);

    //Đếm số lượng bài học hoàn thành của khóa học
    Task<int> GetCompletedLessonsCountAsync(int studentId, int courseId);

    //Tính % hoàn thành khóa học của sinh viên
    Task<double> CalculateProgressPercentageAsync(int studentId, int courseId);
    Task<bool> IsLessonCompletedAsync(int studentId, int lessonId);
}
