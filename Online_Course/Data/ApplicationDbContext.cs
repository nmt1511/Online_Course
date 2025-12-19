using Microsoft.EntityFrameworkCore;
using Online_Course.Models;

namespace Online_Course.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Role> Roles { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
    public DbSet<Course> Courses { get; set; }
    public DbSet<Lesson> Lessons { get; set; }
    public DbSet<Enrollment> Enrollments { get; set; }
    public DbSet<Progress> Progresses { get; set; }
    public DbSet<Category> Categories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Role configuration
        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("roles");
            entity.HasKey(r => r.RoleId);
            entity.Property(r => r.RoleId).HasColumnName("role_id");
            entity.Property(r => r.Name).HasColumnName("name").IsRequired().HasMaxLength(50);
            entity.HasIndex(r => r.Name).IsUnique();
        });

        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(u => u.UserId);
            entity.Property(u => u.UserId).HasColumnName("user_id");
            entity.Property(u => u.FullName).HasColumnName("full_name").IsRequired().HasMaxLength(100);
            entity.Property(u => u.Email).HasColumnName("email").IsRequired().HasMaxLength(256);
            entity.Property(u => u.PasswordHash).HasColumnName("password_hash").IsRequired();
            entity.Property(u => u.CreatedAt).HasColumnName("created_at");
            entity.Property(u => u.IsActive).HasColumnName("is_active");
            entity.Property(u => u.ResetToken).HasColumnName("reset_token");
            entity.Property(u => u.ResetTokenExpiry).HasColumnName("reset_token_expiry");
            entity.HasIndex(u => u.Email).IsUnique();
        });

        // UserRole configuration (many-to-many relationship)
        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.ToTable("user_roles");
            entity.HasKey(ur => ur.UserRoleId);
            entity.Property(ur => ur.UserRoleId).HasColumnName("user_role_id");
            entity.Property(ur => ur.UserId).HasColumnName("user_id");
            entity.Property(ur => ur.RoleId).HasColumnName("role_id");
            entity.HasIndex(ur => new { ur.UserId, ur.RoleId }).IsUnique();
            
            entity.HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Course configuration
        modelBuilder.Entity<Course>(entity =>
        {
            entity.ToTable("courses");
            entity.HasKey(c => c.CourseId);
            entity.Property(c => c.CourseId).HasColumnName("course_id");
            entity.Property(c => c.Title).HasColumnName("title").IsRequired().HasMaxLength(200);
            entity.Property(c => c.Description).HasColumnName("description").HasMaxLength(2000);
            entity.Property(c => c.Category).HasColumnName("category").HasMaxLength(100);
            entity.Property(c => c.ThumbnailUrl).HasColumnName("thumbnail_url").HasMaxLength(500);
            entity.Property(c => c.CreatedBy).HasColumnName("created_by");
            entity.Property(c => c.CourseStatus).HasColumnName("course_status");
            entity.Property(c => c.CourseType).HasColumnName("course_type");
            entity.Property(c => c.StartDate).HasColumnName("start_date");
            entity.Property(c => c.EndDate).HasColumnName("end_date");
            entity.Property(c => c.RegistrationStartDate).HasColumnName("registration_start_date");
            entity.Property(c => c.RegistrationEndDate).HasColumnName("registration_end_date");
            entity.Property(c => c.CategoryId).HasColumnName("category_id");

            entity.HasIndex(c => c.Category);
            entity.HasIndex(c => c.CourseStatus);
            entity.HasIndex(c => c.CourseType);

            entity.HasOne(c => c.Instructor)
                .WithMany(u => u.CreatedCourses)
                .HasForeignKey(c => c.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(c => c.CategoryEntity)
                .WithMany()
                .HasForeignKey(c => c.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Lesson configuration
        modelBuilder.Entity<Lesson>(entity =>
        {
            entity.ToTable("lessons");
            entity.HasKey(l => l.LessonId);
            entity.Property(l => l.LessonId).HasColumnName("lesson_id");
            entity.Property(l => l.CourseId).HasColumnName("course_id");
            entity.Property(l => l.Title).HasColumnName("title").IsRequired().HasMaxLength(200);
            entity.Property(l => l.Description).HasColumnName("description").HasColumnType("nvarchar(max)");
            entity.Property(l => l.ContentUrl).HasColumnName("content_url").HasMaxLength(500);
            entity.Property(l => l.LessonType).HasColumnName("lesson_type");
            entity.Property(l => l.TotalDurationSeconds).HasColumnName("total_duration_seconds");
            entity.Property(l => l.TotalPages).HasColumnName("total_pages");
            entity.Property(l => l.OrderIndex).HasColumnName("order_index");
            entity.Property(l => l.CreatedAt).HasColumnName("created_at");
            entity.Property(l => l.UpdatedAt).HasColumnName("updated_at");
            
            entity.HasIndex(l => new { l.CourseId, l.OrderIndex });
            
            entity.HasOne(l => l.Course)
                .WithMany(c => c.Lessons)
                .HasForeignKey(l => l.CourseId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Enrollment configuration
        modelBuilder.Entity<Enrollment>(entity =>
        {
            entity.ToTable("enrollments");
            entity.HasKey(e => e.EnrollmentId);
            entity.Property(e => e.EnrollmentId).HasColumnName("enrollment_id");
            entity.Property(e => e.CourseId).HasColumnName("course_id");
            entity.Property(e => e.StudentId).HasColumnName("student_id");
            entity.Property(e => e.EnrolledAt).HasColumnName("enrolled_at");
            entity.Property(e => e.LearningStatus).HasColumnName("learning_status");
            entity.Property(e => e.ProgressPercent).HasColumnName("progress_percent");
            entity.Property(e => e.IsMandatory).HasColumnName("is_mandatory");
            
            entity.HasIndex(e => new { e.CourseId, e.StudentId }).IsUnique();
            
            entity.HasOne(e => e.Course)
                .WithMany(c => c.Enrollments)
                .HasForeignKey(e => e.CourseId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.Student)
                .WithMany(u => u.Enrollments)
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Progress configuration
        modelBuilder.Entity<Progress>(entity =>
        {
            entity.ToTable("progresses");
            entity.HasKey(p => p.ProgressId);
            entity.Property(p => p.ProgressId).HasColumnName("progress_id");
            entity.Property(p => p.LessonId).HasColumnName("lesson_id");
            entity.Property(p => p.StudentId).HasColumnName("student_id");
            entity.Property(p => p.IsCompleted).HasColumnName("is_completed");
            entity.Property(p => p.CurrentTimeSeconds).HasColumnName("current_time_seconds");
            entity.Property(p => p.CurrentPage).HasColumnName("current_page");
            entity.Property(p => p.LastUpdate).HasColumnName("last_update");
            
            entity.HasIndex(p => new { p.LessonId, p.StudentId }).IsUnique();
            
            entity.HasOne(p => p.Lesson)
                .WithMany(l => l.Progresses)
                .HasForeignKey(p => p.LessonId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(p => p.Student)
                .WithMany(u => u.Progresses)
                .HasForeignKey(p => p.StudentId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        // Category configuration
        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("categories");
            entity.HasKey(c => c.CategoryId);
            entity.Property(c => c.CategoryId).HasColumnName("category_id");
            entity.Property(c => c.Name).HasColumnName("name").IsRequired().HasMaxLength(100);
            entity.Property(c => c.Slug).HasColumnName("slug").HasMaxLength(100);
            entity.Property(c => c.CreatedAt).HasColumnName("created_at");
            entity.Property(c => c.UpdatedAt).HasColumnName("updated_at");
            entity.Property(c => c.IsActive).HasColumnName("is_active");
            
            entity.HasIndex(c => c.Name).IsUnique();
            entity.HasIndex(c => c.Slug).IsUnique();
        });
    }
}

