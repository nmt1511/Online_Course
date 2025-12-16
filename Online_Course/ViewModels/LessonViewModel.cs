using System.ComponentModel.DataAnnotations;

namespace Online_Course.ViewModels;

public class LessonViewModel
{
    public int LessonId { get; set; }
    
    public int CourseId { get; set; }
    
    [Required(ErrorMessage = "Tiêu đề bài học là bắt buộc")]
    [StringLength(200, ErrorMessage = "Tiêu đề không được vượt quá 200 ký tự")]
    public string Title { get; set; } = string.Empty;
    
    [StringLength(2000, ErrorMessage = "Mô tả không được vượt quá 2000 ký tự")]
    public string Description { get; set; } = string.Empty;
    
    [Url(ErrorMessage = "URL video không hợp lệ")]
    [StringLength(500, ErrorMessage = "URL không được vượt quá 500 ký tự")]
    public string VideoUrl { get; set; } = string.Empty;
    
    public int OrderIndex { get; set; }
    
    // For display purposes
    public string? CourseTitle { get; set; }
}

public class LessonListViewModel
{
    public int CourseId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public IEnumerable<LessonViewModel> Lessons { get; set; } = new List<LessonViewModel>();
}
