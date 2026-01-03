using Online_Course.Models;

namespace Online_Course.ViewModels;

public class LearningLessonViewModel
{
    public int LessonId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ContentUrl { get; set; } = string.Empty;
    public LessonType LessonType { get; set; }
    public int OrderIndex { get; set; }
    public bool IsCompleted { get; set; }
    public bool IsLocked { get; set; } // Trạng thái giới hạn quyền truy cập bài học (Khóa bài học)
}

public class LearningLessonsViewModel
{
    public int CourseId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public int? CourseCategoryId { get; set; }
    public string CourseCategoryName { get; set; } = string.Empty;
    public int TotalLessons { get; set; }
    public int CompletedLessons { get; set; }
    public double ProgressPercentage { get; set; }
    public LearningStatus CourseStatus { get; set; } // Ghi nhận trạng thái tiến trình học tập của học viên đối với khóa học này
    public bool IsCourseClosed { get; set; } // Trạng thái vận hành hiện tại của khóa học (Đóng hoặc Mở)
    public IList<LearningLessonViewModel> Lessons { get; set; } = new List<LearningLessonViewModel>();
}

public class LearningContentViewModel
{
    public int LessonId { get; set; }
    public string LessonTitle { get; set; } = string.Empty;
    public string LessonDescription { get; set; } = string.Empty;
    public string ContentUrl { get; set; } = string.Empty;
    public LessonType LessonType { get; set; }
    public int OrderIndex { get; set; }
    public bool IsCompleted { get; set; }
    
    public int CourseId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public int? CourseCategoryId { get; set; }
    public string CourseCategoryName { get; set; } = string.Empty;
    public string InstructorName { get; set; } = string.Empty;
    
    public int TotalLessons { get; set; }
    public int CompletedLessons { get; set; }
    public double ProgressPercentage { get; set; }
    public bool IsCourseClosed { get; set; } // Trạng thái quản lý vận hành của khóa học (Đóng hoặc Mở)
    
    public int? CurrentTimeSeconds { get; set; }
    public int? CurrentPage { get; set; }
    public int? TotalDurationSeconds { get; set; }
    public int? TotalPages { get; set; }
    
    public int? PreviousLessonId { get; set; }
    public int? NextLessonId { get; set; }
    
    public IList<LearningLessonViewModel> AllLessons { get; set; } = new List<LearningLessonViewModel>();
}
