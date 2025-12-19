using Microsoft.EntityFrameworkCore;
using Online_Course.Models;
using System.Security.Cryptography;
using System.Text;

namespace Online_Course.Data;

// Using CourseStatus enum from Models

public static class DbInitializer
{
    public static async Task InitializeAsync(ApplicationDbContext context)
    {
        // Ensure database is created
        await context.Database.EnsureCreatedAsync();

        // Seed Roles
        await SeedRolesAsync(context);

        // Seed Categories
        await SeedCategoriesAsync(context);

        // Seed Users
        await SeedUsersAsync(context);

        // Seed Courses
        await SeedCoursesAsync(context);

        // Seed Lessons
        await SeedLessonsAsync(context);

        // Seed Enrollments and Progress
        await SeedEnrollmentsAndProgressAsync(context);
    }

    private static async Task SeedRolesAsync(ApplicationDbContext context)
    {
        // Add roles if they don't exist
        var existingRoles = await context.Roles.Select(r => r.Name).ToListAsync();
        var rolesToAdd = new[] { "Admin", "Instructor", "Student" }
            .Where(r => !existingRoles.Contains(r))
            .Select(r => new Role { Name = r })
            .ToList();

        if (rolesToAdd.Any())
        {
            await context.Roles.AddRangeAsync(rolesToAdd);
            await context.SaveChangesAsync();
        }
    }

    private static async Task SeedCategoriesAsync(ApplicationDbContext context)
    {
        // Add categories if they don't exist
        var existingCategories = await context.Categories.Select(c => c.Name).ToListAsync();
        var categoriesToAdd = new[]
        {
            new { Name = "Marketing", Slug = "marketing" },
            new { Name = "Kinh Doanh Online", Slug = "kinh-doanh-online" },
            new { Name = "Data Science & AI", Slug = "data-science-ai" },
            new { Name = "Lập Trình", Slug = "lap-trinh" },
            new { Name = "Thiết Kế", Slug = "thiet-ke" },
            new { Name = "Tài Chính & Đầu Tư", Slug = "tai-chinh-dau-tu" },
            new { Name = "Business Analyst", Slug = "business-analyst" },
            new { Name = "Ngoại Ngữ", Slug = "ngoai-ngu" },
            new { Name = "Phát Triển Bản Thân", Slug = "phat-trien-ban-than" },
            new { Name = "Video & Content", Slug = "video-content" }
        }
        .Where(c => !existingCategories.Contains(c.Name))
        .Select(c => new Category { Name = c.Name, Slug = c.Slug, IsActive = true })
        .ToList();

        if (categoriesToAdd.Any())
        {
            await context.Categories.AddRangeAsync(categoriesToAdd);
            await context.SaveChangesAsync();
        }
    }

    private static async Task SeedUsersAsync(ApplicationDbContext context)
    {
        var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
        var instructorRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Instructor");
        var studentRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Student");
        
        if (adminRole == null || instructorRole == null || studentRole == null) return;

        // Admin user
        if (!await context.Users.AnyAsync(u => u.Email == "admin@onlinecourse.com"))
        {
            var admin = new User
            {
                FullName = "System Administrator",
                Email = "admin@onlinecourse.com",
                PasswordHash = HashPassword("Admin@123"),
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };
            await context.Users.AddAsync(admin);
            await context.SaveChangesAsync();
            await context.UserRoles.AddAsync(new UserRole { UserId = admin.UserId, RoleId = adminRole.RoleId });
        }

        // Instructors
        var instructors = new[]
        {
            new { FullName = "Nguyễn Văn Hùng", Email = "hung.nguyen@onlinecourse.com" },
            new { FullName = "Trần Thị Mai", Email = "mai.tran@onlinecourse.com" },
            new { FullName = "Phạm Đăng Định", Email = "dinh.pham@onlinecourse.com" },
            new { FullName = "Lê Minh Tuấn", Email = "tuan.le@onlinecourse.com" },
            new { FullName = "Nguyễn Thanh Nam", Email = "nam.nguyen@onlinecourse.com" }
        };

        foreach (var inst in instructors)
        {
            if (!await context.Users.AnyAsync(u => u.Email == inst.Email))
            {
                var user = new User
                {
                    FullName = inst.FullName,
                    Email = inst.Email,
                    PasswordHash = HashPassword("Instructor@123"),
                    CreatedAt = DateTime.UtcNow.AddDays(-Random.Shared.Next(30, 365)),
                    IsActive = true
                };
                await context.Users.AddAsync(user);
                await context.SaveChangesAsync();
                await context.UserRoles.AddAsync(new UserRole { UserId = user.UserId, RoleId = instructorRole.RoleId });
            }
        }

        // Students
        var students = new[]
        {
            new { FullName = "Hoàng Văn An", Email = "an.hoang@gmail.com" },
            new { FullName = "Nguyễn Thị Bình", Email = "binh.nguyen@gmail.com" },
            new { FullName = "Trần Văn Cường", Email = "cuong.tran@gmail.com" },
            new { FullName = "Lê Thị Dung", Email = "dung.le@gmail.com" },
            new { FullName = "Phạm Văn Em", Email = "em.pham@gmail.com" },
            new { FullName = "Võ Thị Phương", Email = "phuong.vo@gmail.com" },
            new { FullName = "Đặng Văn Giang", Email = "giang.dang@gmail.com" },
            new { FullName = "Bùi Thị Hoa", Email = "hoa.bui@gmail.com" },
            new { FullName = "Ngô Văn Khoa", Email = "khoa.ngo@gmail.com" },
            new { FullName = "Đinh Thị Lan", Email = "lan.dinh@gmail.com" }
        };

        foreach (var stu in students)
        {
            if (!await context.Users.AnyAsync(u => u.Email == stu.Email))
            {
                var user = new User
                {
                    FullName = stu.FullName,
                    Email = stu.Email,
                    PasswordHash = HashPassword("Student@123"),
                    CreatedAt = DateTime.UtcNow.AddDays(-Random.Shared.Next(1, 180)),
                    IsActive = true
                };
                await context.Users.AddAsync(user);
                await context.SaveChangesAsync();
                await context.UserRoles.AddAsync(new UserRole { UserId = user.UserId, RoleId = studentRole.RoleId });
            }
        }

        await context.SaveChangesAsync();
    }


    private static async Task SeedCoursesAsync(ApplicationDbContext context)
    {
        var instructors = await context.Users
            .Where(u => u.UserRoles.Any(ur => ur.Role.Name == "Instructor"))
            .ToListAsync();

        if (!instructors.Any()) return;
        
        // Get existing course titles to avoid duplicates
        var existingTitles = await context.Courses.Select(c => c.Title).ToListAsync();
        
        // Get categories for linking
        var categories = await context.Categories.ToListAsync();

        // Courses based on Kho Khóa Học data
        var coursesData = new[]
        {
            // Marketing courses
            new { Title = "Facebook Ads Pro 2025 - Tăng Doanh Thu Với QC Livestream", Category = "Marketing", 
                  Description = "Khóa học quảng cáo Facebook chuyên sâu, hướng dẫn chạy ads livestream hiệu quả, tối ưu chi phí quảng cáo và tăng doanh thu bán hàng.", 
                  Thumbnail = "/images/courses/facebook-ads.png", InstructorIndex = 0 },
            new { Title = "Google Ads Nâng Cao - Chiến Lược Quảng Cáo Hiệu Quả", Category = "Marketing", 
                  Description = "Học cách tối ưu chiến dịch Google Ads, nghiên cứu từ khóa, viết quảng cáo hấp dẫn và phân tích hiệu suất.", 
                  Thumbnail = "/images/courses/google-ads.png", InstructorIndex = 0 },
            new { Title = "TikTok Ads 2025 - Ứng Dụng AI Làm Video Bán Hàng", Category = "Marketing", 
                  Description = "Khóa học chạy quảng cáo TikTok hiệu quả, sử dụng AI để tạo video bán hàng viral, thu hút khách hàng tiềm năng.", 
                  Thumbnail = "/images/courses/tiktok-ads.png", InstructorIndex = 1 },
            new { Title = "Marketing Automation & Analytics", Category = "Marketing", 
                  Description = "Tự động hóa marketing, phân tích dữ liệu khách hàng, xây dựng funnel bán hàng tự động.", 
                  Thumbnail = "/images/courses/marketing-automation.png", InstructorIndex = 1 },
            
            // Data Science & AI courses
            new { Title = "Data Science & Machine Learning với Python", Category = "Data Science & AI", 
                  Description = "Khóa học toàn diện về khoa học dữ liệu và machine learning, từ cơ bản đến nâng cao với Python.", 
                  Thumbnail = "/images/courses/data-science.png", InstructorIndex = 2 },
            new { Title = "Data Analytics 101 - Phân Tích Dữ Liệu Cho Người Mới", Category = "Data Science & AI", 
                  Description = "Nhập môn phân tích dữ liệu, học Excel, SQL và các công cụ visualization cơ bản.", 
                  Thumbnail = "/images/courses/data-analytics.png", InstructorIndex = 2 },
            new { Title = "AI SEO & Ads Chuyển Đổi", Category = "Data Science & AI", 
                  Description = "Ứng dụng trí tuệ nhân tạo trong SEO và quảng cáo, tối ưu tỷ lệ chuyển đổi.", 
                  Thumbnail = "/images/courses/ai-seo.png", InstructorIndex = 2 },
            new { Title = "ChatGPT for Software Testing", Category = "Data Science & AI", 
                  Description = "Sử dụng ChatGPT để tự động hóa kiểm thử phần mềm, viết test cases và debug code.", 
                  Thumbnail = "/images/courses/chatgpt-testing.png", InstructorIndex = 3 },
            
            // Business Analyst courses
            new { Title = "Business Analyst Foundation - FPT Software Academy", Category = "Business Analyst", 
                  Description = "Nền tảng Business Analyst, học cách phân tích yêu cầu, viết tài liệu và làm việc với stakeholders.", 
                  Thumbnail = "/images/courses/ba-foundation.png", InstructorIndex = 3 },
            new { Title = "Business Analysis for Banking & Fintech", Category = "Business Analyst", 
                  Description = "Chuyên sâu BA trong lĩnh vực ngân hàng và fintech, hiểu quy trình nghiệp vụ tài chính.", 
                  Thumbnail = "/images/courses/ba-banking.png", InstructorIndex = 3 },
            new { Title = "IT Business Analysis Documentation In Practice", Category = "Business Analyst", 
                  Description = "Thực hành viết tài liệu BA chuyên nghiệp: BRD, FRD, Use Cases, User Stories.", 
                  Thumbnail = "/images/courses/ba-documentation.png", InstructorIndex = 4 },
            
            // Finance courses
            new { Title = "Đầu Tư Forex Cho Người Mới Bắt Đầu", Category = "Tài Chính & Đầu Tư", 
                  Description = "Nhập môn giao dịch Forex, phân tích kỹ thuật cơ bản, quản lý vốn và rủi ro.", 
                  Thumbnail = "/images/courses/forex.png", InstructorIndex = 4 },
            new { Title = "Price Action Thực Chiến - Giao Dịch Chứng Khoán", Category = "Tài Chính & Đầu Tư", 
                  Description = "Phương pháp Price Action trong giao dịch chứng khoán, đọc biểu đồ và xác định điểm vào lệnh.", 
                  Thumbnail = "/images/courses/price-action.png", InstructorIndex = 4 },
            new { Title = "Phân Tích BCTC & Financial Modeling", Category = "Tài Chính & Đầu Tư", 
                  Description = "Đọc hiểu báo cáo tài chính, xây dựng mô hình tài chính và định giá doanh nghiệp.", 
                  Thumbnail = "/images/courses/financial-modeling.png", InstructorIndex = 0 },
            new { Title = "Nghiên Cứu Thị Trường Crypto 2025", Category = "Tài Chính & Đầu Tư", 
                  Description = "Phân tích thị trường tiền điện tử, đánh giá dự án blockchain và chiến lược đầu tư.", 
                  Thumbnail = "/images/courses/crypto.png", InstructorIndex = 1 },
            
            // Design & Video courses
            new { Title = "Photoshop Cơ Bản Đến Nâng Cao", Category = "Thiết Kế", 
                  Description = "Học Photoshop từ A-Z, chỉnh sửa ảnh chuyên nghiệp, thiết kế banner và poster.", 
                  Thumbnail = "/images/courses/photoshop.png", InstructorIndex = 1 },
            new { Title = "CapCut Mobile Mastery - Chỉnh Sửa Video Chuyên Nghiệp", Category = "Video & Content", 
                  Description = "Thành thạo CapCut trên điện thoại, tạo video viral cho TikTok, Reels và YouTube Shorts.", 
                  Thumbnail = "/images/courses/capcut.png", InstructorIndex = 2 },
            new { Title = "Tạo Video Thời Trang Bằng AI Miễn Phí", Category = "Video & Content", 
                  Description = "Sử dụng công cụ AI để tạo video thời trang chuyên nghiệp mà không cần kỹ năng edit.", 
                  Thumbnail = "/images/courses/ai-video.png", InstructorIndex = 3 },
            new { Title = "Content Foundation - QCC Mastery Hub", Category = "Video & Content", 
                  Description = "Nền tảng sáng tạo nội dung, xây dựng chiến lược content marketing hiệu quả.", 
                  Thumbnail = "/images/courses/content-foundation.png", InstructorIndex = 4 },
            
            // E-commerce courses
            new { Title = "Dropship POD Brand Builder", Category = "Kinh Doanh Online", 
                  Description = "Xây dựng thương hiệu Print-on-Demand, kinh doanh dropshipping không cần vốn lớn.", 
                  Thumbnail = "/images/courses/dropship.png", InstructorIndex = 0 },
            new { Title = "Kiếm Tiền Affiliate Với TikTok Shop Bằng AI", Category = "Kinh Doanh Online", 
                  Description = "Hướng dẫn kiếm tiền affiliate marketing trên TikTok Shop, sử dụng AI tạo content.", 
                  Thumbnail = "/images/courses/tiktok-affiliate.png", InstructorIndex = 1 },
            new { Title = "Viral Builder 2025 - Xây Dựng Nội Dung Viral", Category = "Kinh Doanh Online", 
                  Description = "Chiến lược tạo nội dung viral, thu hút hàng triệu lượt xem và chuyển đổi thành doanh thu.", 
                  Thumbnail = "/images/courses/viral-builder.png", InstructorIndex = 2 },
            new { Title = "Xây Dựng Hệ Thống CSKH Tự Động Với Zalo OA", Category = "Kinh Doanh Online", 
                  Description = "Tự động hóa chăm sóc khách hàng qua Zalo Official Account, tăng tỷ lệ chốt đơn.", 
                  Thumbnail = "/images/courses/zalo-oa.png", InstructorIndex = 3 },
            
            // Programming courses
            new { Title = "Trở Thành Kỹ Sư Dữ Liệu - Data Engineer", Category = "Lập Trình", 
                  Description = "Lộ trình trở thành Data Engineer, học ETL, data pipeline và cloud platforms.", 
                  Thumbnail = "/images/courses/data-engineer.png", InstructorIndex = 4 },
            new { Title = "Tạo BOT Chứng Khoán Với Python", Category = "Lập Trình", 
                  Description = "Lập trình bot giao dịch chứng khoán tự động, phân tích dữ liệu thị trường với Python.", 
                  Thumbnail = "/images/courses/stock-bot.png", InstructorIndex = 0 }
        };

        var coursesToAdd = new List<Course>();
        foreach (var courseData in coursesData)
        {
            // Skip if course already exists
            if (existingTitles.Contains(courseData.Title)) continue;
            
            var instructor = instructors[courseData.InstructorIndex % instructors.Count];
            var category = categories.FirstOrDefault(c => c.Name == courseData.Category);
            var course = new Course
            {
                Title = courseData.Title,
                Description = courseData.Description,
                CategoryId = category?.CategoryId,
                ThumbnailUrl = courseData.Thumbnail,
                CreatedBy = instructor.UserId,
                CourseStatus = CourseStatus.Public
            };
            coursesToAdd.Add(course);
        }

        if (coursesToAdd.Any())
        {
            await context.Courses.AddRangeAsync(coursesToAdd);
            await context.SaveChangesAsync();
        }
    }


    private static async Task SeedLessonsAsync(ApplicationDbContext context)
    {
        //Nếu đã có bất kỳ bài học nào trong DB thì dừng lại
        if (await context.Lessons.AnyAsync()) return;

        // Get courses that don't have lessons yet
        var coursesWithLessons = await context.Lessons.Select(l => l.CourseId).Distinct().ToListAsync();
        var courses = await context.Courses.Where(c => !coursesWithLessons.Contains(c.CourseId)).ToListAsync();
        if (!courses.Any()) return;

        var lessonTemplates = new Dictionary<string, string[]>
        {
            ["Marketing"] = new[]
            {
                "Giới thiệu khóa học và tổng quan",
                "Thiết lập tài khoản quảng cáo",
                "Nghiên cứu đối tượng mục tiêu",
                "Tạo chiến dịch quảng cáo đầu tiên",
                "Viết nội dung quảng cáo hấp dẫn",
                "Thiết kế hình ảnh và video quảng cáo",
                "Tối ưu ngân sách và đấu giá",
                "Phân tích và đọc báo cáo",
                "A/B Testing và tối ưu hóa",
                "Scaling chiến dịch thành công"
            },
            ["Data Science & AI"] = new[]
            {
                "Giới thiệu về Data Science",
                "Cài đặt môi trường làm việc",
                "Python cơ bản cho Data Science",
                "Xử lý dữ liệu với Pandas",
                "Trực quan hóa dữ liệu",
                "Thống kê mô tả và suy luận",
                "Machine Learning cơ bản",
                "Supervised Learning",
                "Unsupervised Learning",
                "Dự án thực tế và tổng kết"
            },
            ["Business Analyst"] = new[]
            {
                "Tổng quan về Business Analysis",
                "Vai trò và trách nhiệm của BA",
                "Kỹ thuật thu thập yêu cầu",
                "Phân tích và mô hình hóa quy trình",
                "Viết User Stories và Use Cases",
                "Tạo wireframe và prototype",
                "Quản lý yêu cầu và thay đổi",
                "Làm việc với Agile/Scrum",
                "Kỹ năng giao tiếp và thuyết trình",
                "Dự án thực hành"
            },
            ["Tài Chính & Đầu Tư"] = new[]
            {
                "Giới thiệu thị trường tài chính",
                "Các khái niệm cơ bản",
                "Phân tích cơ bản (Fundamental)",
                "Phân tích kỹ thuật (Technical)",
                "Đọc biểu đồ và mô hình giá",
                "Quản lý vốn và rủi ro",
                "Tâm lý giao dịch",
                "Xây dựng chiến lược giao dịch",
                "Backtest và tối ưu hóa",
                "Thực hành giao dịch demo"
            },
            ["Thiết Kế"] = new[]
            {
                "Giới thiệu công cụ thiết kế",
                "Giao diện và các công cụ cơ bản",
                "Làm việc với layers và masks",
                "Chỉnh sửa màu sắc và ánh sáng",
                "Retouching và làm đẹp ảnh",
                "Thiết kế banner và poster",
                "Typography và bố cục",
                "Xuất file và định dạng",
                "Workflow chuyên nghiệp",
                "Dự án thực tế"
            },
            ["Video & Content"] = new[]
            {
                "Giới thiệu về content creation",
                "Lên ý tưởng và kịch bản",
                "Quay video cơ bản",
                "Cắt ghép và chỉnh sửa video",
                "Thêm hiệu ứng và transition",
                "Chỉnh màu và âm thanh",
                "Tạo thumbnail hấp dẫn",
                "Tối ưu SEO cho video",
                "Phân phối nội dung đa nền tảng",
                "Phân tích và cải thiện"
            },
            ["Kinh Doanh Online"] = new[]
            {
                "Tổng quan kinh doanh online",
                "Nghiên cứu thị trường và sản phẩm",
                "Xây dựng thương hiệu cá nhân",
                "Thiết lập kênh bán hàng",
                "Tạo nội dung bán hàng",
                "Chiến lược giá và khuyến mãi",
                "Quản lý đơn hàng và vận chuyển",
                "Chăm sóc khách hàng",
                "Phân tích doanh thu và lợi nhuận",
                "Mở rộng quy mô kinh doanh"
            },
            ["Lập Trình"] = new[]
            {
                "Giới thiệu và cài đặt môi trường",
                "Cú pháp và kiểu dữ liệu cơ bản",
                "Cấu trúc điều khiển",
                "Hàm và module",
                "Lập trình hướng đối tượng",
                "Xử lý file và exception",
                "Làm việc với API",
                "Database và SQL",
                "Testing và debugging",
                "Deploy và bảo trì"
            },
            ["Ngoại Ngữ"] = new[]
            {
                "Giới thiệu khóa học",
                "Phát âm cơ bản",
                "Ngữ pháp nền tảng",
                "Từ vựng theo chủ đề",
                "Kỹ năng nghe",
                "Kỹ năng nói",
                "Kỹ năng đọc",
                "Kỹ năng viết",
                "Giao tiếp thực tế",
                "Ôn tập và kiểm tra"
            },
            ["Phát Triển Bản Thân"] = new[]
            {
                "Xác định mục tiêu cá nhân",
                "Quản lý thời gian hiệu quả",
                "Xây dựng thói quen tốt",
                "Kỹ năng giao tiếp",
                "Tư duy tích cực",
                "Quản lý stress",
                "Làm việc nhóm",
                "Lãnh đạo bản thân",
                "Networking và quan hệ",
                "Lập kế hoạch phát triển"
            }
        };

        // Load categories for mapping
        var categoriesDict = await context.Categories.ToDictionaryAsync(c => c.CategoryId, c => c.Name);
        
        foreach (var course in courses)
        {
            var categoryName = course.CategoryId.HasValue && categoriesDict.ContainsKey(course.CategoryId.Value) 
                ? categoriesDict[course.CategoryId.Value] 
                : "Lập Trình";
            var lessons = lessonTemplates.ContainsKey(categoryName) 
                ? lessonTemplates[categoryName] 
                : lessonTemplates["Lập Trình"];

            for (int i = 0; i < lessons.Length; i++)
            {
                var lesson = new Lesson
                {
                    CourseId = course.CourseId,
                    Title = $"Bài {i + 1}: {lessons[i]}",
                    Description = $"Nội dung chi tiết về {lessons[i].ToLower()} trong khóa học {course.Title}.",
                    ContentUrl = $"https://example.com/videos/course-{course.CourseId}/lesson-{i + 1}.mp4",
                    OrderIndex = i + 1
                };
                await context.Lessons.AddAsync(lesson);
            }
        }

        await context.SaveChangesAsync();
    }

    private static async Task SeedEnrollmentsAndProgressAsync(ApplicationDbContext context)
    {
        //Nếu đã có dữ liệu đăng ký hoặc tiến trình thì dừng
        if (await context.Enrollments.AnyAsync() || await context.Progresses.AnyAsync())
            return;

        var students = await context.Users
            .Where(u => u.UserRoles.Any(ur => ur.Role.Name == "Student"))
            .ToListAsync();
        
        if (!students.Any()) return;
        
        // Get existing enrollments
        var existingEnrollments = await context.Enrollments
            .Select(e => new { e.StudentId, e.CourseId })
            .ToListAsync();

        var courses = await context.Courses
            .Include(c => c.Lessons)
            .Where(c => c.CourseStatus == CourseStatus.Public)
            .ToListAsync();

        if (!courses.Any()) return;

        var random = new Random(42); // Fixed seed for reproducibility

        foreach (var student in students)
        {
            // Each student enrolls in 3-6 random courses
            var enrollCount = random.Next(3, 7);
            var selectedCourses = courses.OrderBy(_ => random.Next()).Take(enrollCount).ToList();

            foreach (var course in selectedCourses)
            {
                // Skip if already enrolled
                if (existingEnrollments.Any(e => e.StudentId == student.UserId && e.CourseId == course.CourseId))
                    continue;
                    
                var enrollment = new Enrollment
                {
                    CourseId = course.CourseId,
                    StudentId = student.UserId,
                    EnrolledAt = DateTime.UtcNow.AddDays(-random.Next(1, 90))
                };
                await context.Enrollments.AddAsync(enrollment);
                await context.SaveChangesAsync();

                // Create progress for some lessons (0-100% completion)
                var completionRate = random.NextDouble();
                var lessonsToComplete = (int)(course.Lessons.Count * completionRate);

                foreach (var lesson in course.Lessons.OrderBy(l => l.OrderIndex).Take(lessonsToComplete))
                {
                    // Check if progress already exists
                    var progressExists = await context.Progresses
                        .AnyAsync(p => p.LessonId == lesson.LessonId && p.StudentId == student.UserId);
                    if (progressExists) continue;
                    
                    var progress = new Progress
                    {
                        LessonId = lesson.LessonId,
                        StudentId = student.UserId,
                        IsCompleted = true,
                        LastUpdate = DateTime.UtcNow.AddDays(-random.Next(1, 30))
                    };
                    await context.Progresses.AddAsync(progress);
                }
            }
        }

        await context.SaveChangesAsync();
    }

    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }
}
