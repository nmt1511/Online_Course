using Online_Course.Models;

namespace Online_Course.Services;

public interface IProgressService
{
    Task<IEnumerable<Progress>> GetProgressByStudentAndCourseAsync(int studentId, int courseId);

    //Đếm số lượng bài học hoàn thành của khóa học
    Task<int> GetCompletedLessonsCountAsync(int studentId, int courseId);

    //Tính % hoàn thành khóa học của sinh viên
    Task<double> CalculateProgressPercentageAsync(int studentId, int courseId);

    //Kiểm tra bài học đã hoàn thành ?
    Task<bool> IsLessonCompletedAsync(int studentId, int lessonId);

    // Cập nhật tiến độ chi tiết 
    Task<Progress> UpdateProgressAsync(int studentId, int lessonId, int? currentTime, int? currentPage, bool isCompleted);
}
