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
    
    /// <summary>
    /// URL của video (dùng khi LessonType = Video)
    /// Hỗ trợ YouTube và các nền tảng video khác
    /// </summary>
    [StringLength(500, ErrorMessage = "URL không được vượt quá 500 ký tự")]
    public string? VideoUrl { get; set; }
    
    /// <summary>
    /// Loại bài học: PDF hoặc Video
    /// </summary>
    public LessonType LessonType { get; set; } = LessonType.Video;
    
    /// <summary>
    /// Tổng số trang (chỉ dùng cho PDF)
    /// Tự động đếm khi upload file
    /// </summary>
    public int? TotalPages { get; set; }
    
    /// <summary>
    /// Tổng thời lượng video tính bằng giây (chỉ dùng cho Video)
    /// Tự động lấy từ YouTube API nếu có API key
    /// </summary>
    public int? TotalDurationSeconds { get; set; }
    
    /// <summary>
    /// Thứ tự bài học trong khóa học
    /// Tự động tính = số bài học hiện có + 1
    /// </summary>
    public int OrderIndex { get; set; }
    
    // For display purposes
    public string? CourseTitle { get; set; }
    
    /// <summary>
    /// Đường dẫn đến file PDF (để hiển thị khi edit)
    /// </summary>
    public string? PdfUrl { get; set; }
}

public class LessonListViewModel
{
    public int CourseId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public IEnumerable<LessonViewModel> Lessons { get; set; } = new List<LessonViewModel>();
}
