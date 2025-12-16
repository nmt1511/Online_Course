# Online Course Platform

Hệ thống quản lý khóa học trực tuyến được xây dựng bằng ASP.NET Core MVC 8.0.

## 📋 Mô tả

Ứng dụng web quản lý khóa học trực tuyến với 3 vai trò chính:
- **Admin**: Quản lý người dùng, danh mục, khóa học
- **Instructor**: Tạo và quản lý khóa học, bài học, theo dõi tiến độ học viên
- **Student**: Đăng ký khóa học, học bài, theo dõi tiến độ học tập

## 🛠️ Công nghệ sử dụng

- **Framework**: ASP.NET Core MVC 8.0
- **Database**: SQL Server với Entity Framework Core
- **Authentication**: Cookie Authentication
- **Frontend**: Bootstrap, jQuery, jQuery Validation
- **Architecture**: MVC với Areas pattern

## 📁 Cấu trúc dự án

```
Online_Course/
├── Areas/                    # Phân chia theo vai trò
│   ├── Admin/               # Quản trị viên
│   ├── Instructor/          # Giảng viên
│   └── Student/             # Học viên
├── Controllers/             # Controllers chung (Home, Account)
├── Models/                  # Domain models
├── ViewModels/              # View models cho UI
├── Services/                # Business logic layer
├── Data/                    # Database context và migrations
├── Views/                   # Razor views
└── wwwroot/                 # Static files (CSS, JS, images)
```

## 🚀 Cài đặt và chạy

### Yêu cầu hệ thống
- .NET 8.0 SDK
- SQL Server (LocalDB hoặc SQL Server Express)
- Visual Studio 2022 hoặc VS Code

### Các bước cài đặt

1. **Clone repository**
```bash
git clone <repository-url>
cd Online_Course
```

2. **Cấu hình database**
   - Mở file `Online_Course/appsettings.json`
   - Cập nhật connection string:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=OnlineCourseDB;Trusted_Connection=True;MultipleActiveResultSets=true"
  }
}
```

3. **Chạy migrations**
```bash
cd Online_Course
dotnet ef database update
```

4. **Chạy ứng dụng**
```bash
dotnet run
```

Hoặc chạy từ Visual Studio:
- Nhấn F5 hoặc chọn "Start Debugging"

5. **Truy cập ứng dụng**
   - Mở trình duyệt và truy cập: `https://localhost:5001` hoặc `http://localhost:5000`

## 👥 Vai trò và quyền

### Admin
- Quản lý người dùng (thêm, sửa, xóa)
- Quản lý danh mục khóa học
- Quản lý tất cả khóa học
- Xem báo cáo tổng quan

### Instructor
- Tạo và quản lý khóa học của mình
- Tạo và quản lý bài học
- Xem danh sách học viên và tiến độ học tập
- Sửa thông tin khóa học

### Student
- Đăng ký khóa học
- Xem danh sách khóa học đã đăng ký
- Học bài và theo dõi tiến độ
- Xem nội dung bài học

## 📦 Các tính năng chính

- ✅ Đăng ký / Đăng nhập người dùng
- ✅ Phân quyền theo vai trò (Admin, Instructor, Student)
- ✅ Quản lý khóa học và bài học
- ✅ Đăng ký khóa học
- ✅ Theo dõi tiến độ học tập
- ✅ Dashboard cho từng vai trò
- ✅ Quản lý danh mục khóa học

## 🧪 Testing

Project có kèm test project `Online_Course.Tests` với các property-based tests.

Chạy tests:
```bash
dotnet test
```

## 📝 Ghi chú

- Database sẽ được tự động seed với dữ liệu mẫu khi khởi động lần đầu
- File `.gitignore` đã được cấu hình để loại trừ các thư mục build và file không cần thiết

## 📄 License

[MIT License](LICENSE)

## 👨‍💻 Tác giả

[Your Name/Team Name]

---

**Lưu ý**: Đây là project học tập/dự án mẫu. Vui lòng cập nhật thông tin license và tác giả phù hợp với dự án của bạn.

"# Online_Course" 
