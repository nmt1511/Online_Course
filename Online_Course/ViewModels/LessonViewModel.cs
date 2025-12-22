using System.ComponentModel.DataAnnotations;
using Online_Course.Models;

namespace Online_Course.ViewModels;

/// <summary>
/// ViewModel cho tạo/chỉnh sửa bài học
/// Hỗ trợ 2 loại: PDF và Video
/// </summary>
public class LessonViewModel
{
    public int LessonId { get; set; }
    
    public int CourseId { get; set; }
    
    [Required(ErrorMessage = "Tiêu đề bài học là bắt buộc")]
    [StringLength(200, ErrorMessage = "Tiêu đề không được vượt quá 200 ký tự")]
    public string Title { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    
    // URL của video (dùng khi LessonType = Video)
    // Hỗ trợ YouTube và các nền tảng video khác
    [StringLength(500, ErrorMessage = "URL không được vượt quá 500 ký tự")]
    public string? VideoUrl { get; set; }
    
    // Loại bài học: PDF hoặc Video
    public LessonType LessonType { get; set; } = LessonType.Video;
    
    //Tổng số trang (chỉ dùng cho PDF)
    // Tự động đếm khi upload file
    public int? TotalPages { get; set; }
    
    //Tổng thời lượng video tính bằng giây (chỉ dùng cho Video)
    //Tự động lấy từ YouTube API nếu có API key
    public int? TotalDurationSeconds { get; set; }
    
    //Thứ tự bài học trong khóa học
    //Tự động tính = số bài học hiện có + 1
    public int OrderIndex { get; set; }
    
    // For display purposes
    public string? CourseTitle { get; set; }
    
    //Đường dẫn đến file PDF (để hiển thị khi edit)
    public string? PdfUrl { get; set; }
}

public class LessonListViewModel
{
    public int CourseId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public IEnumerable<LessonViewModel> Lessons { get; set; } = new List<LessonViewModel>();
}
