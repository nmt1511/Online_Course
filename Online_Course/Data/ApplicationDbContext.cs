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
            entity.HasKey(r => r.RoleId);
            entity.HasIndex(r => r.Name).IsUnique();
            entity.Property(r => r.Name).IsRequired().HasMaxLength(50);
        });

        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.UserId);
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.FullName).IsRequired().HasMaxLength(100);
            entity.Property(u => u.Email).IsRequired().HasMaxLength(256);
            entity.Property(u => u.PasswordHash).IsRequired();
        });

        // UserRole configuration (many-to-many relationship)
        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(ur => ur.UserRoleId);
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
            entity.HasKey(c => c.CourseId);
            entity.Property(c => c.Title).IsRequired().HasMaxLength(200);
            entity.Property(c => c.Description).HasMaxLength(2000);
            entity.Property(c => c.Category).HasMaxLength(100);
            entity.Property(c => c.ThumbnailUrl).HasMaxLength(500);
            entity.HasIndex(c => c.Category);
            
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
            entity.HasKey(l => l.LessonId);
            entity.Property(l => l.Title).IsRequired().HasMaxLength(200);
            entity.Property(l => l.Description).HasColumnType("nvarchar(max)");
            entity.Property(l => l.ContentUrl).HasMaxLength(500);
            entity.HasIndex(l => new { l.CourseId, l.OrderIndex });
            
            entity.HasOne(l => l.Course)
                .WithMany(c => c.Lessons)
                .HasForeignKey(l => l.CourseId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Enrollment configuration
        modelBuilder.Entity<Enrollment>(entity =>
        {
            entity.HasKey(e => e.EnrollmentId);
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
            entity.HasKey(p => p.ProgressId);
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
            entity.HasKey(c => c.CategoryId);
            entity.HasIndex(c => c.Name).IsUnique();
            entity.HasIndex(c => c.Slug).IsUnique();
            entity.Property(c => c.Name).IsRequired().HasMaxLength(100);
            entity.Property(c => c.Slug).HasMaxLength(100);
        });
    }
}
