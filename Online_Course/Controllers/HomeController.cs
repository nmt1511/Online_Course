using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Online_Course.Models;
using Online_Course.Services.CategoryService;
using Online_Course.Services.CourseService;
using Online_Course.ViewModels;

namespace Online_Course.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ICourseService _courseService;
        private readonly ICategoryService _categoryService;

        public HomeController(ILogger<HomeController> logger, ICourseService courseService, ICategoryService categoryService)
        {
            _logger = logger;
            _courseService = courseService;
            _categoryService = categoryService;
        }

        // Hiển thị trang chủ với danh sách 8 khóa học mới nhất và các danh mục hoạt động
        public async Task<IActionResult> Index()
        {
            var courses = await _courseService.GetAllCoursesAsync();
            var categories = await _categoryService.GetAllCategoriesAsync();
            
            var viewModel = new HomeViewModel
            {
                FeaturedCourses = courses.Take(8).Select(c => new HomeCourseViewModel
                {
                    CourseId = c.CourseId,
                    Title = c.Title,
                    Description = c.Description,
                    ThumbnailUrl = c.ThumbnailUrl,
                    CategoryName = c.CategoryEntity?.Name ?? "Chưa phân loại",
                    InstructorName = c.Instructor?.FullName ?? "Giảng viên",
                    LessonCount = c.Lessons?.Count ?? 0
                }).ToList(),
                Categories = categories.Where(c => c.IsActive).Select(c => new HomeCategoryViewModel
                {
                    CategoryId = c.CategoryId,
                    Name = c.Name,
                    Slug = c.Slug
                }).ToList(),
                TotalCourses = courses.Count(),
                TotalCategories = categories.Count()
            };
            
            return View(viewModel);
        }

        // Hiển thị trang thông tin về chính sách bảo mật
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        // Xử lý và hiển thị thông tin lỗi chi tiết
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
