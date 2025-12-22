using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Online_Course.Models;
using Online_Course.Services;
using Online_Course.ViewModels;
using System.Security.Claims;

namespace Online_Course.Areas.Instructor.Controllers;

/// <summary>
/// Controller quản lý bài học cho Instructor
/// Hỗ trợ 2 loại bài học: PDF và Video
/// </summary>
[Area("Instructor")]
[Authorize(Policy = "InstructorOnly")]
public class LessonsController : Controller
{
    private readonly ILessonService _lessonService;
    private readonly ICourseService _courseService;
    private readonly IPdfService _pdfService;
    private readonly IYouTubeApiService _youTubeApiService;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly ILogger<LessonsController> _logger;

    public LessonsController(
        ILessonService lessonService,
        ICourseService courseService,
        IPdfService pdfService,
        IYouTubeApiService youTubeApiService,
        IWebHostEnvironment webHostEnvironment,
        ILogger<LessonsController> logger)
    {
        _lessonService = lessonService;
        _courseService = courseService;
        _pdfService = pdfService;
        _youTubeApiService = youTubeApiService;
        _webHostEnvironment = webHostEnvironment;
        _logger = logger;
    }

    /// <summary>
    /// Lấy ID của user hiện tại từ claims
    /// </summary>
    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        return userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;
    }

    #region Index - Danh sách bài học

    /// <summary>
    /// GET: Instructor/Lessons - Hiển thị danh sách bài học theo khóa học
    /// </summary>
    public async Task<IActionResult> Index(int? courseId = null)
    {
        var instructorId = GetCurrentUserId();

        if (courseId.HasValue)
        {
            // Kiểm tra khóa học tồn tại và thuộc về instructor này
            var course = await _courseService.GetCourseByIdAsync(courseId.Value);
            if (course == null)
                return NotFound();

            if (course.CreatedBy != instructorId)
                return Forbid();

            // Lấy danh sách bài học
            var lessons = await _lessonService.GetLessonsByCourseAsync(courseId.Value);

            var viewModel = new LessonListViewModel
            {
                CourseId = courseId.Value,
                CourseTitle = course.Title,
                Lessons = lessons.Select(l => new LessonViewModel
                {
                    LessonId = l.LessonId,
                    CourseId = l.CourseId,
                    Title = l.Title,
                    Description = l.Description,
                    VideoUrl = l.LessonType == LessonType.Video ? l.ContentUrl : null,
                    PdfUrl = l.LessonType == LessonType.Pdf ? l.ContentUrl : null,
                    LessonType = l.LessonType,
                    TotalPages = l.TotalPages,
                    TotalDurationSeconds = l.TotalDurationSeconds,
                    OrderIndex = l.OrderIndex
                })
            };

            return View(viewModel);
        }
        else
        {
            // Redirect về trang danh sách khóa học nếu không có courseId
            return RedirectToAction("Index", "Courses");
        }
    }

    #endregion

    #region Create - Tạo bài học mới

    /// <summary>
    /// GET: Instructor/Lessons/Create/{courseId}
    /// Hiển thị form tạo bài học mới
    /// </summary>
    public async Task<IActionResult> Create(int courseId)
    {
        var course = await _courseService.GetCourseByIdAsync(courseId);
        if (course == null)
            return NotFound();

        if (course.CreatedBy != GetCurrentUserId())
            return Forbid();

        // Tạo ViewModel với OrderIndex tự động (số bài học + 1)
        var nextOrderIndex = await _lessonService.GetNextOrderIndexAsync(courseId);
        
        var viewModel = new LessonViewModel
        {
            CourseId = courseId,
            CourseTitle = course.Title,
            OrderIndex = nextOrderIndex,
            LessonType = LessonType.Video // Mặc định là Video
        };

        _logger.LogInformation("[LessonsController.Create GET] CourseId: {CourseId}, NextOrderIndex: {OrderIndex}",
            courseId, nextOrderIndex);

        return View(viewModel);
    }

    /// <summary>
    /// POST: Instructor/Lessons/Create
    /// Xử lý tạo bài học mới (PDF hoặc Video)
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(LessonViewModel model, IFormFile? pdfFile)
    {
        _logger.LogInformation("[LessonsController.Create POST] CourseId: {CourseId}, LessonType: {LessonType}, Title: {Title}",
            model.CourseId, model.LessonType, model.Title);

        // Kiểm tra quyền truy cập khóa học
        var course = await _courseService.GetCourseByIdAsync(model.CourseId);
        if (course == null)
            return NotFound();

        if (course.CreatedBy != GetCurrentUserId())
            return Forbid();

        // Validate theo loại bài học
        if (model.LessonType == LessonType.Pdf)
        {
            // PDF: Phải có file upload
            if (pdfFile == null || pdfFile.Length == 0)
            {
                ModelState.AddModelError("pdfFile", "Vui lòng chọn file PDF để upload.");
            }
            // Xóa validation cho VideoUrl vì không cần
            ModelState.Remove("VideoUrl");
        }
        else // Video
        {
            // Video: Phải có URL
            if (string.IsNullOrWhiteSpace(model.VideoUrl))
            {
                ModelState.AddModelError("VideoUrl", "Vui lòng nhập URL của video.");
            }
        }

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("[LessonsController.Create POST] ModelState invalid. Errors: {Errors}",
                string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
            
            model.CourseTitle = course.Title;
            return View(model);
        }

        // Tạo entity Lesson
        var lesson = new Lesson
        {
            CourseId = model.CourseId,
            Title = model.Title,
            Description = model.Description ?? string.Empty,
            LessonType = model.LessonType,
            // OrderIndex sẽ tự động tính trong LessonService
        };

        try
        {
            if (model.LessonType == LessonType.Pdf)
            {
                // === XỬ LÝ PDF ===
                _logger.LogInformation("[LessonsController.Create POST] Đang xử lý upload PDF: {FileName}", pdfFile!.FileName);

                // 1. Lưu file PDF vào wwwroot/pdf_lessons
                var pdfPath = await _pdfService.SavePdfAsync(pdfFile);
                lesson.ContentUrl = pdfPath;

                // 2. Đếm số trang PDF
                var absolutePath = Path.Combine(_webHostEnvironment.WebRootPath, pdfPath.TrimStart('/'));
                lesson.TotalPages = _pdfService.CountPages(absolutePath);

                _logger.LogInformation("[LessonsController.Create POST] PDF saved: {Path}, Pages: {Pages}",
                    pdfPath, lesson.TotalPages);
            }
            else // Video
            {
                // === XỬ LÝ VIDEO ===
                lesson.ContentUrl = model.VideoUrl!;

                // Thử lấy duration từ YouTube API (nếu là YouTube URL và có API key)
                if (_youTubeApiService.IsYouTubeUrl(model.VideoUrl!))
                {
                    _logger.LogInformation("[LessonsController.Create POST] Đang lấy duration từ YouTube API cho: {Url}", model.VideoUrl);
                    
                    var duration = await _youTubeApiService.GetVideoDurationSecondsAsync(model.VideoUrl!);
                    lesson.TotalDurationSeconds = duration;

                    if (duration.HasValue)
                    {
                        _logger.LogInformation("[LessonsController.Create POST] YouTube duration: {Seconds} giây", duration);
                    }
                    else
                    {
                        _logger.LogWarning("[LessonsController.Create POST] Không thể lấy duration từ YouTube API. " +
                            "Có thể API key chưa được cấu hình hoặc URL không hợp lệ.");
                    }
                }
                else
                {
                    _logger.LogInformation("[LessonsController.Create POST] URL không phải YouTube, bỏ qua lấy duration tự động.");
                }
            }

            // Lưu bài học vào database
            await _lessonService.CreateLessonAsync(lesson);

            _logger.LogInformation("[LessonsController.Create POST] Tạo bài học thành công. LessonId: {LessonId}", lesson.LessonId);
            
            TempData["SuccessMessage"] = "Bài học đã được tạo thành công!";
            return RedirectToAction(nameof(Index), new { courseId = model.CourseId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[LessonsController.Create POST] Lỗi khi tạo bài học");
            ModelState.AddModelError("", "Có lỗi xảy ra khi tạo bài học. Vui lòng thử lại.");
            model.CourseTitle = course.Title;
            return View(model);
        }
    }

    //Edit - Chỉnh sửa bài học

    /// <summary>
    /// GET: Instructor/Lessons/Edit/{id}
    /// </summary>
    public async Task<IActionResult> Edit(int id)
    {
        _logger.LogInformation("[LessonsController.Edit GET] LessonId: {LessonId}", id);

        var lesson = await _lessonService.GetLessonByIdAsync(id);
        if (lesson == null)
        {
            _logger.LogWarning("[LessonsController.Edit GET] Lesson not found: {LessonId}", id);
            return NotFound();
        }

        var course = await _courseService.GetCourseByIdAsync(lesson.CourseId);
        if (course == null || course.CreatedBy != GetCurrentUserId())
        {
            _logger.LogWarning("[LessonsController.Edit GET] Forbidden access to lesson: {LessonId}", id);
            return Forbid();
        }

        var viewModel = new LessonViewModel
        {
            LessonId = lesson.LessonId,
            CourseId = lesson.CourseId,
            Title = lesson.Title,
            Description = lesson.Description,
            LessonType = lesson.LessonType,
            VideoUrl = lesson.LessonType == LessonType.Video ? lesson.ContentUrl : null,
            PdfUrl = lesson.LessonType == LessonType.Pdf ? lesson.ContentUrl : null,
            TotalPages = lesson.TotalPages,
            TotalDurationSeconds = lesson.TotalDurationSeconds,
            OrderIndex = lesson.OrderIndex,
            CourseTitle = course.Title
        };

        return View(viewModel);
    }

    /// <summary>
    /// POST: Instructor/Lessons/Edit
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(LessonViewModel model, IFormFile? pdfFile)
    {
        _logger.LogInformation("[LessonsController.Edit POST] LessonId: {LessonId}, LessonType: {LessonType}",
            model.LessonId, model.LessonType);

        var existingLesson = await _lessonService.GetLessonByIdAsync(model.LessonId);
        if (existingLesson == null)
            return NotFound();

        var courseId = model.CourseId > 0 ? model.CourseId : existingLesson.CourseId;
        var course = await _courseService.GetCourseByIdAsync(courseId);
        
        if (course == null || course.CreatedBy != GetCurrentUserId())
            return Forbid();

        // Validate theo loại bài học
        if (model.LessonType == LessonType.Pdf)
        {
            // PDF: Có thể giữ file cũ hoặc upload file mới
            ModelState.Remove("VideoUrl");
        }
        else
        {
            if (string.IsNullOrWhiteSpace(model.VideoUrl))
            {
                ModelState.AddModelError("VideoUrl", "Vui lòng nhập URL của video.");
            }
        }

        if (!ModelState.IsValid)
        {
            model.CourseId = courseId;
            model.CourseTitle = course.Title;
            return View(model);
        }

        try
        {
            var lesson = new Lesson
            {
                LessonId = model.LessonId,
                CourseId = courseId,
                Title = model.Title,
                Description = model.Description ?? string.Empty,
                LessonType = model.LessonType,
                OrderIndex = existingLesson.OrderIndex // Giữ nguyên OrderIndex
            };

            if (model.LessonType == LessonType.Pdf)
            {
                // Nếu có file mới, lưu file mới
                if (pdfFile != null && pdfFile.Length > 0)
                {
                    var pdfPath = await _pdfService.SavePdfAsync(pdfFile);
                    lesson.ContentUrl = pdfPath;

                    var absolutePath = Path.Combine(_webHostEnvironment.WebRootPath, pdfPath.TrimStart('/'));
                    lesson.TotalPages = _pdfService.CountPages(absolutePath);

                    _logger.LogInformation("[LessonsController.Edit POST] New PDF uploaded: {Path}, Pages: {Pages}",
                        pdfPath, lesson.TotalPages);
                }
                else
                {
                    // Giữ file cũ
                    lesson.ContentUrl = existingLesson.ContentUrl;
                    lesson.TotalPages = existingLesson.TotalPages;
                }
            }
            else // Video
            {
                lesson.ContentUrl = model.VideoUrl!;

                // Lấy duration nếu URL thay đổi
                if (existingLesson.ContentUrl != model.VideoUrl && _youTubeApiService.IsYouTubeUrl(model.VideoUrl!))
                {
                    lesson.TotalDurationSeconds = await _youTubeApiService.GetVideoDurationSecondsAsync(model.VideoUrl!);
                }
                else
                {
                    lesson.TotalDurationSeconds = existingLesson.TotalDurationSeconds;
                }
            }

            await _lessonService.UpdateLessonAsync(lesson);

            TempData["SuccessMessage"] = "Bài học đã được cập nhật thành công!";
            return RedirectToAction(nameof(Index), new { courseId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[LessonsController.Edit POST] Lỗi khi cập nhật bài học: {LessonId}", model.LessonId);
            ModelState.AddModelError("", "Có lỗi xảy ra khi cập nhật bài học.");
            model.CourseId = courseId;
            model.CourseTitle = course.Title;
            return View(model);
        }
    }

    #endregion

    #region Delete - Xóa bài học

    /// <summary>
    /// POST: Instructor/Lessons/Delete/{id}
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        _logger.LogInformation("[LessonsController.Delete] LessonId: {LessonId}", id);

        var lesson = await _lessonService.GetLessonByIdAsync(id);
        if (lesson == null)
            return NotFound();

        var course = await _courseService.GetCourseByIdAsync(lesson.CourseId);
        if (course == null || course.CreatedBy != GetCurrentUserId())
            return Forbid();

        var courseId = lesson.CourseId;

        // Xóa file PDF nếu có
        if (lesson.LessonType == LessonType.Pdf && !string.IsNullOrEmpty(lesson.ContentUrl))
        {
            try
            {
                var filePath = Path.Combine(_webHostEnvironment.WebRootPath, lesson.ContentUrl.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                    _logger.LogInformation("[LessonsController.Delete] Đã xóa file PDF: {FilePath}", filePath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[LessonsController.Delete] Không thể xóa file PDF: {Url}", lesson.ContentUrl);
            }
        }

        await _lessonService.DeleteLessonAsync(id);

        TempData["SuccessMessage"] = "Bài học đã được xóa thành công!";
        return RedirectToAction(nameof(Index), new { courseId });
    }

    #endregion

    #region Reorder - Sắp xếp lại thứ tự bài học

    /// <summary>
    /// POST: Instructor/Lessons/Reorder
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reorder(int courseId, [FromBody] int[] lessonIds)
    {
        var course = await _courseService.GetCourseByIdAsync(courseId);
        if (course == null || course.CreatedBy != GetCurrentUserId())
            return Forbid();

        await _lessonService.ReorderLessonsAsync(courseId, lessonIds);
        return Ok();
    }

    #endregion
}
