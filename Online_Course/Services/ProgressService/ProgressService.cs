using Microsoft.EntityFrameworkCore;
using Online_Course.Data;
using Online_Course.Helper;
using Online_Course.Models;

namespace Online_Course.Services.ProgressService;

public class ProgressService : IProgressService
{
    private readonly ApplicationDbContext _context;

    public ProgressService(ApplicationDbContext context)
    {
        _context = context;
    }

    // Lấy danh sách tiến độ theo sinh viên và khóa học
    public async Task<IEnumerable<Progress>> GetProgressByStudentAndCourseAsync(int studentId, int courseId)
    {
        return await _context.Progresses
            .Include(p => p.Lesson)
            .Where(p => p.StudentId == studentId && p.Lesson.CourseId == courseId)
            .ToListAsync();
    }

    // Đếm số lượng bài học đã hoàn thành của một khóa học
    public async Task<int> GetCompletedLessonsCountAsync(int studentId, int courseId)
    {
        return await _context.Progresses
            .Include(p => p.Lesson)
            .CountAsync(p => p.StudentId == studentId 
                && p.Lesson.CourseId == courseId 
                && p.IsCompleted);
    }

    // Tính % hoàn thành khóa học dựa trên tiến độ chi tiết của từng bài học
    public async Task<double> CalculateProgressPercentageAsync(int studentId, int courseId)
    {
        // Lấy danh sách tất cả bài học trong khóa học
        var lessons = await _context.Lessons
            .Where(l => l.CourseId == courseId)
            .ToListAsync();

        if (!lessons.Any())
            return 0;

        // Lấy tiến độ của học viên cho các bài học đó
        var progresses = await _context.Progresses
            .Where(p => p.StudentId == studentId && lessons.Select(l => l.LessonId).Contains(p.LessonId))
            .ToListAsync();

        double totalWeightedProgress = 0;

        foreach (var lesson in lessons)
        {
            var progress = progresses.FirstOrDefault(p => p.LessonId == lesson.LessonId);
            if (progress == null) continue;

            if (progress.IsCompleted)
            {
                // Nếu bài học đã hoàn thành thì cộng 100%
                totalWeightedProgress += 100;
            }
            else
            {
                // Tính tỷ lệ % dựa trên thời gian video hoặc số trang PDF đang xem
                if (lesson.LessonType == LessonType.Video && lesson.TotalDurationSeconds > 0)
                {
                    totalWeightedProgress += (double)(progress.CurrentTimeSeconds ?? 0) / lesson.TotalDurationSeconds.Value * 100;
                }
                else if (lesson.LessonType == LessonType.Pdf && lesson.TotalPages > 0)
                {
                    totalWeightedProgress += (double)(progress.CurrentPage ?? 0) / lesson.TotalPages.Value * 100;
                }
            }
        }

        // Trả về giá trị trung bình cộng
        return totalWeightedProgress / lessons.Count;
    }

    // Cập nhật tiến độ chi tiết (thời gian xem video/trang PDF)
    public async Task<Progress> UpdateProgressAsync(int studentId, int lessonId, int? currentTime, int? currentPage, bool isCompleted)
    {
        // Lấy tiến độ hiện tại kèm theo thông tin bài học
        var progress = await _context.Progresses
            .Include(p => p.Lesson)
            .FirstOrDefaultAsync(p => p.StudentId == studentId && p.LessonId == lessonId);

        if (progress == null)
        {
            // Nếu chưa có tiến độ thì tạo mới
            var lesson = await _context.Lessons.FindAsync(lessonId);
            progress = new Progress
            {
                StudentId = studentId,
                LessonId = lessonId,
                Lesson = lesson!
            };
            _context.Progresses.Add(progress);
        }
        else if (progress.IsCompleted)
        {
            // Nếu bài học đã hoàn thành rồi thì không thể cập nhật tiến độ nữa
            return progress;
        }

        // Cập nhật có điều kiện: chỉ cập nhật nếu tiến độ mới lớn hơn tiến độ cũ
        if (currentTime.HasValue && currentTime > (progress.CurrentTimeSeconds ?? 0))
        {
            progress.CurrentTimeSeconds = currentTime;
        }

        if (currentPage.HasValue && currentPage > (progress.CurrentPage ?? 0))
        {
            progress.CurrentPage = currentPage;
        }
        
        // Đánh dấu hoàn thành nếu có yêu cầu từ frontend
        if (isCompleted)
        {
            progress.IsCompleted = true;
        }

        // Tự động cộng dồn hoàn thành nếu gần hết video (ngưỡng 5 giây) hoặc đến trang cuối PDF
        if (!progress.IsCompleted)
        {
            if (progress.Lesson.LessonType == LessonType.Video && progress.Lesson.TotalDurationSeconds > 0)
            {
                if (progress.CurrentTimeSeconds >= progress.Lesson.TotalDurationSeconds - 5)
                {
                    progress.IsCompleted = true;
                }
            }
            else if (progress.Lesson.LessonType == LessonType.Pdf && progress.Lesson.TotalPages > 0)
            {
                if (progress.CurrentPage >= progress.Lesson.TotalPages)
                {
                    progress.IsCompleted = true;
                }
            }
        }
        
        progress.LastUpdate = DateTimeHelper.GetVietnamTimeNow();
        await _context.SaveChangesAsync();

        // Cập nhật trạng thái và lưu % tiến độ vào bảng Enrollment
        await UpdateCourseStatusAsync(studentId, progress.Lesson.CourseId);
        await _context.SaveChangesAsync();

        return progress;
    }

    // Cập nhật trạng thái tổng thể của khóa học (LearningStatus)
    private async Task UpdateCourseStatusAsync(int studentId, int courseId)
    {
        var enroll = await _context.Enrollments.FirstOrDefaultAsync(e => e.StudentId == studentId && e.CourseId == courseId);
        if (enroll == null) return;

        // Nếu đang ở trạng thái chưa học thì chuyển sang đang học
        if (enroll.LearningStatus == LearningStatus.NOT_STARTED)
        {
            enroll.LearningStatus = LearningStatus.IN_PROGRESS;
        }

        // Cập nhật % tiến độ thực tế vào bảng Enrollment
        enroll.ProgressPercent = (float)await CalculateProgressPercentageAsync(studentId, courseId);

        // Kiểm tra nếu tất cả bài học đã hoàn thành thì đánh dấu khóa học hoàn thành
        var totalLessons = await _context.Lessons.CountAsync(l => l.CourseId == courseId);
        var completedCount = await GetCompletedLessonsCountAsync(studentId, courseId);

        if (totalLessons > 0 && completedCount == totalLessons)
        {
            enroll.LearningStatus = LearningStatus.COMPLETED;
        }
    }

    // Kiểm tra một bài học cụ thể đã hoàn thành chưa
    public async Task<bool> IsLessonCompletedAsync(int studentId, int lessonId)
    {
        return await _context.Progresses
            .AnyAsync(p => p.StudentId == studentId && p.LessonId == lessonId && p.IsCompleted);
    }
}
