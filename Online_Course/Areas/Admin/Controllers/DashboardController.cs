using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Online_Course.Services;

namespace Online_Course.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "AdminOnly")]
public class DashboardController : Controller
{
    private readonly IUserService _userService;
    private readonly ICourseService _courseService;
    private readonly ICategoryService _categoryService;

    public DashboardController(
        IUserService userService,
        ICourseService courseService,
        ICategoryService categoryService)
    {
        _userService = userService;
        _courseService = courseService;
        _categoryService = categoryService;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.TotalUsers = await _userService.GetTotalUsersCountAsync();
        ViewBag.TotalCourses = (await _courseService.GetAllCoursesAsync()).Count();
        ViewBag.TotalCategories = await _categoryService.GetTotalCategoriesCountAsync();
        ViewBag.ActiveCategories = await _categoryService.GetActiveCategoriesCountAsync();
        
        return View();
    }
}
