using Online_Course.Helper;

namespace Online_Course.Models;

public class Progress
{
    public int ProgressId { get; set; }
    public int LessonId { get; set; }
    public int StudentId { get; set; }

    // Trạng thái hoàn thành của bài học
    public bool IsCompleted { get; set; } = false; 

    // Thời gian hiện tại của người dùng trong bài học (tính bằng giây)
    public int? CurrentTimeSeconds { get; set; } 

    // Trang hiện tại của người dùng trong bài học
    public int? CurrentPage { get; set; }

    // Thời gian cập nhật lần cuối của tiến độ bài học
    public DateTime LastUpdate { get; set; } = DateTimeHelper.GetVietnamTimeNow(); 
    public Lesson Lesson { get; set; } = null!;
    public User Student { get; set; } = null!;
}
