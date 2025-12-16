using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Online_Course.Models;
using Online_Course.Services;
using Online_Course.ViewModels;
using System.Security.Claims;

namespace Online_Course.Areas.Instructor.Controllers;

[Area("Instructor")]
[Authorize(Policy = "InstructorOnly")]
public class LessonsController : Controller
{
    private readonly ILessonService _lessonService;
    private readonly ICourseService _courseService;

    public LessonsController(ILessonService lessonService, ICourseService courseService)
    {
        _lessonService = lessonService;
        _courseService = courseService;
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        return userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;
    }

    // GET: Instructor/Lessons - Show all lessons grouped by course
    public async Task<IActionResult> Index(int? courseId = null)
    {
        var instructorId = GetCurrentUserId();
        
        if (courseId.HasValue)
        {
            // Show lessons for specific course
            var course = await _courseService.GetCourseByIdAsync(courseId.Value);
            if (course == null)
                return NotFound();

            if (course.CreatedBy != instructorId)
                return Forbid();

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
                    VideoUrl = l.VideoUrl,
                    OrderIndex = l.OrderIndex
                })
            };

            return View(viewModel);
        }
        else
        {
            // Show all courses with lesson counts - redirect to course selection
            return RedirectToAction("Index", "Courses");
        }
    }

    // GET: Instructor/Lessons/Create/{courseId}
    public async Task<IActionResult> Create(int courseId)
    {
        var course = await _courseService.GetCourseByIdAsync(courseId);
        if (course == null)
            return NotFound();

        if (course.CreatedBy != GetCurrentUserId())
            return Forbid();

        var viewModel = new LessonViewModel
        {
            CourseId = courseId,
            CourseTitle = course.Title,
            OrderIndex = await _lessonService.GetNextOrderIndexAsync(courseId)
        };

        return View(viewModel);
    }

    // POST: Instructor/Lessons/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(LessonViewModel model)
    {
        var course = await _courseService.GetCourseByIdAsync(model.CourseId);
        if (course == null)
            return NotFound();

        if (course.CreatedBy != GetCurrentUserId())
            return Forbid();

        if (ModelState.IsValid)
        {
            var lesson = new Lesson
            {
                CourseId = model.CourseId,
                Title = model.Title,
                Description = model.Description,
                VideoUrl = model.VideoUrl,
                OrderIndex = model.OrderIndex
            };

            await _lessonService.CreateLessonAsync(lesson);
            TempData["SuccessMessage"] = "Bài học đã được tạo thành công!";
            return RedirectToAction(nameof(Index), new { courseId = model.CourseId });
        }

        model.CourseTitle = course.Title;
        return View(model);
    }


    // GET: Instructor/Lessons/Edit/{id}
    public async Task<IActionResult> Edit(int id)
    {
        Console.WriteLine($"[Edit GET] LessonId: {id}, CurrentUserId: {GetCurrentUserId()}");
        
        var lesson = await _lessonService.GetLessonByIdAsync(id);
        if (lesson == null)
        {
            Console.WriteLine($"[Edit GET] Lesson not found: {id}");
            return NotFound();
        }

        Console.WriteLine($"[Edit GET] Lesson found - CourseId: {lesson.CourseId}");
        
        var course = await _courseService.GetCourseByIdAsync(lesson.CourseId);
        if (course == null)
        {
            Console.WriteLine($"[Edit GET] Course not found: {lesson.CourseId}");
            return Forbid();
        }
        
        Console.WriteLine($"[Edit GET] Course found - CreatedBy: {course.CreatedBy}, CurrentUser: {GetCurrentUserId()}");
        
        if (course.CreatedBy != GetCurrentUserId())
        {
            Console.WriteLine($"[Edit GET] Forbidden - Course owner: {course.CreatedBy}, Current user: {GetCurrentUserId()}");
            return Forbid();
        }

        var viewModel = new LessonViewModel
        {
            LessonId = lesson.LessonId,
            CourseId = lesson.CourseId,
            Title = lesson.Title,
            Description = lesson.Description,
            VideoUrl = lesson.VideoUrl,
            OrderIndex = lesson.OrderIndex,
            CourseTitle = course.Title
        };

        Console.WriteLine($"[Edit GET] Returning view with model: LessonId={viewModel.LessonId}, CourseId={viewModel.CourseId}, Title={viewModel.Title}");
        
        try
        {
            return View(viewModel);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Edit GET] View Error: {ex.Message}");
            Console.WriteLine($"[Edit GET] Stack: {ex.StackTrace}");
            throw;
        }
    }

    // POST: Instructor/Lessons/Edit
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(LessonViewModel model)
    {
        // Debug: Log received data
        Console.WriteLine($"[Edit POST] LessonId: {model.LessonId}, CourseId: {model.CourseId}, Title: {model.Title}");
        
        var existingLesson = await _lessonService.GetLessonByIdAsync(model.LessonId);
        if (existingLesson == null)
        {
            Console.WriteLine($"[Edit POST] Lesson not found: {model.LessonId}");
            return NotFound();
        }

        // Use existing lesson's CourseId if model.CourseId is 0
        var courseId = model.CourseId > 0 ? model.CourseId : existingLesson.CourseId;
        
        var course = await _courseService.GetCourseByIdAsync(courseId);
        if (course == null || course.CreatedBy != GetCurrentUserId())
        {
            Console.WriteLine($"[Edit POST] Forbidden - CourseId: {courseId}");
            return Forbid();
        }

        if (!ModelState.IsValid)
        {
            // Debug: Log validation errors
            foreach (var error in ModelState)
            {
                foreach (var e in error.Value.Errors)
                {
                    Console.WriteLine($"[Edit POST] Validation Error - {error.Key}: {e.ErrorMessage}");
                }
            }
            model.CourseId = courseId;
            model.CourseTitle = course.Title;
            return View(model);
        }

        var lesson = new Lesson
        {
            LessonId = model.LessonId,
            CourseId = courseId,
            Title = model.Title,
            Description = model.Description ?? string.Empty,
            VideoUrl = model.VideoUrl ?? string.Empty,
            OrderIndex = model.OrderIndex
        };

        await _lessonService.UpdateLessonAsync(lesson);
        TempData["SuccessMessage"] = "Bài học đã được cập nhật thành công!";
        return RedirectToAction(nameof(Index), new { courseId = courseId });
    }

    // POST: Instructor/Lessons/Delete/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var lesson = await _lessonService.GetLessonByIdAsync(id);
        if (lesson == null)
            return NotFound();

        var course = await _courseService.GetCourseByIdAsync(lesson.CourseId);
        if (course == null || course.CreatedBy != GetCurrentUserId())
            return Forbid();

        var courseId = lesson.CourseId;
        await _lessonService.DeleteLessonAsync(id);
        TempData["SuccessMessage"] = "Bài học đã được xóa thành công!";
        return RedirectToAction(nameof(Index), new { courseId });
    }

    // POST: Instructor/Lessons/Reorder
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
}
