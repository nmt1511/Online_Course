using System.ComponentModel.DataAnnotations;
using Online_Course.Models;

namespace Online_Course.ViewModels;

    // Cấu trúc dữ liệu phục vụ việc khởi tạo hoặc cập nhật thông tin bài học
    // Hỗ trợ hai loại hình nội dung chính: Tài liệu PDF và Nội dung Video
public class LessonViewModel
{
    public int LessonId { get; set; }
    
    public int CourseId { get; set; }
    
    [Required(ErrorMessage = "Tiêu đề bài học là bắt buộc")]
    [StringLength(200, ErrorMessage = "Tiêu đề không được vượt quá 200 ký tự")]
    public string Title { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    
    // Đường dẫn liên kết nội dung Video (Áp dụng khi loại bài học là Video)
    // Hệ thống tương thích với nền tảng YouTube và các nguồn phát trực tuyến khác
    [StringLength(500, ErrorMessage = "URL không được vượt quá 500 ký tự")]
    public string? VideoUrl { get; set; }
    
    // Loại bài học: PDF hoặc Video
    public LessonType LessonType { get; set; } = LessonType.Video;
    
    // Tổng hợp số lượng trang tài liệu (Chỉ áp dụng đối với định dạng PDF)
    // Hệ thống tự động phân tích và trích xuất số trang trong quá trình tải tệp tin
    public int? TotalPages { get; set; }
    
    // Tổng thời lượng nội dung video tính theo đơn vị giây (Chỉ áp dụng cho Video)
    // Dữ liệu được truy xuất tự động thông qua YouTube API nếu có khóa ứng dụng hợp lệ
    public int? TotalDurationSeconds { get; set; }
    
    // Thứ tự sắp xếp của bài học trong cấu trúc khóa học
    // Giá trị được mặc định bằng tổng số bài học hiện hữu cộng thêm một đơn vị
    public int OrderIndex { get; set; }
    
    // For display purposes
    public string? CourseTitle { get; set; }
    
    // Đường dẫn truy cập tệp tin PDF (Sử dụng cho mục đích hiển thị trong giao diện chỉnh sửa)
    public string? PdfUrl { get; set; }
}

public class LessonListViewModel
{
    public int CourseId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public IEnumerable<LessonViewModel> Lessons { get; set; } = new List<LessonViewModel>();
}
