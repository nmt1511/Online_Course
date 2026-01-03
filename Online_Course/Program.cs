using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Online_Course.Data;
using Online_Course.Services.CategoryService;
using Online_Course.Services.CourseService;
using Online_Course.Services.EnrollmentService;
using Online_Course.Services.LessonService;
using Online_Course.Services.PdfService;
using Online_Course.Services.ProgressService;
using Online_Course.Services.ReportService;
using Online_Course.Services.UserService;
using Online_Course.Services.YoutubeApiService;

var builder = WebApplication.CreateBuilder(args);

// Đăng ký các dịch vụ hệ thống vào Container của ứng dụng
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Đăng ký các dịch vụ nghiệp vụ (Application Services)
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ILessonService, LessonService>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
builder.Services.AddScoped<IProgressService, ProgressService>();
builder.Services.AddScoped<IReportService, ReportService>();

// Cấu hình các dịch vụ xử lý PDF và YouTube phục vụ tạo bài học
builder.Services.AddScoped<IPdfService, PdfService>();
builder.Services.AddHttpClient<IYouTubeApiService, YouTubeApiService>();

// Thiết lập cơ chế xác thực dựa trên Cookie (Cookie Authentication)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(24);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    });

// Thiết lập các chính sách phân quyền (Authorization Policies)
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("InstructorOnly", policy => policy.RequireRole("Instructor"));
    options.AddPolicy("StudentOnly", policy => policy.RequireRole("Student"));
    options.AddPolicy("InstructorOrAdmin", policy => policy.RequireRole("Admin", "Instructor"));
});

builder.Services.AddControllersWithViews();

// Thêm chính sách CORS cho trình xem PDF / Add CORS policy for PDF viewer
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowPdfViewer",
        policy =>
        {
            policy.WithOrigins("https://mozilla.github.io")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

var app = builder.Build();

// Khởi tạo cơ sở dữ liệu và nạp dữ liệu mẫu (Seed Data)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();
    await DbInitializer.InitializeAsync(context);
}

// Cấu hình quy trình xử lý yêu cầu HTTP (Middleware Pipeline)
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
// Thiết lập quyền truy cập tệp tin tĩnh và cấu hình CORS
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        ctx.Context.Response.Headers.Append("Access-Control-Allow-Origin", "*");
        ctx.Context.Response.Headers.Append("Access-Control-Allow-Headers", "Origin, X-Requested-With, Content-Type, Accept");
    }
});

app.UseRouting();

app.UseCors("AllowPdfViewer");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
