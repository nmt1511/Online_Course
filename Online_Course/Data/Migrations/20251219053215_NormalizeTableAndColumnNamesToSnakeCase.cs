using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Online_Course.Migrations
{
    /// <inheritdoc />
    public partial class NormalizeTableAndColumnNamesToSnakeCase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Courses_Categories_CategoryId",
                table: "Courses");

            migrationBuilder.DropForeignKey(
                name: "FK_Courses_Users_CreatedBy",
                table: "Courses");

            migrationBuilder.DropForeignKey(
                name: "FK_Enrollments_Courses_CourseId",
                table: "Enrollments");

            migrationBuilder.DropForeignKey(
                name: "FK_Enrollments_Users_StudentId",
                table: "Enrollments");

            migrationBuilder.DropForeignKey(
                name: "FK_Lessons_Courses_CourseId",
                table: "Lessons");

            migrationBuilder.DropForeignKey(
                name: "FK_Progresses_Lessons_LessonId",
                table: "Progresses");

            migrationBuilder.DropForeignKey(
                name: "FK_Progresses_Users_StudentId",
                table: "Progresses");

            migrationBuilder.DropForeignKey(
                name: "FK_UserRoles_Roles_RoleId",
                table: "UserRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_UserRoles_Users_UserId",
                table: "UserRoles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Users",
                table: "Users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Roles",
                table: "Roles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Progresses",
                table: "Progresses");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Lessons",
                table: "Lessons");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Enrollments",
                table: "Enrollments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Courses",
                table: "Courses");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Categories",
                table: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_Categories_Slug",
                table: "Categories");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserRoles",
                table: "UserRoles");

            migrationBuilder.RenameTable(
                name: "Users",
                newName: "users");

            migrationBuilder.RenameTable(
                name: "Roles",
                newName: "roles");

            migrationBuilder.RenameTable(
                name: "Progresses",
                newName: "progresses");

            migrationBuilder.RenameTable(
                name: "Lessons",
                newName: "lessons");

            migrationBuilder.RenameTable(
                name: "Enrollments",
                newName: "enrollments");

            migrationBuilder.RenameTable(
                name: "Courses",
                newName: "courses");

            migrationBuilder.RenameTable(
                name: "Categories",
                newName: "categories");

            migrationBuilder.RenameTable(
                name: "UserRoles",
                newName: "user_roles");

            migrationBuilder.RenameColumn(
                name: "Email",
                table: "users",
                newName: "email");

            migrationBuilder.RenameColumn(
                name: "ResetTokenExpiry",
                table: "users",
                newName: "reset_token_expiry");

            migrationBuilder.RenameColumn(
                name: "ResetToken",
                table: "users",
                newName: "reset_token");

            migrationBuilder.RenameColumn(
                name: "PasswordHash",
                table: "users",
                newName: "password_hash");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "users",
                newName: "is_active");

            migrationBuilder.RenameColumn(
                name: "FullName",
                table: "users",
                newName: "full_name");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "users",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "users",
                newName: "user_id");

            migrationBuilder.RenameIndex(
                name: "IX_Users_Email",
                table: "users",
                newName: "IX_users_email");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "roles",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "RoleId",
                table: "roles",
                newName: "role_id");

            migrationBuilder.RenameIndex(
                name: "IX_Roles_Name",
                table: "roles",
                newName: "IX_roles_name");

            migrationBuilder.RenameColumn(
                name: "StudentId",
                table: "progresses",
                newName: "student_id");

            migrationBuilder.RenameColumn(
                name: "LessonId",
                table: "progresses",
                newName: "lesson_id");

            migrationBuilder.RenameColumn(
                name: "LastUpdate",
                table: "progresses",
                newName: "last_update");

            migrationBuilder.RenameColumn(
                name: "IsCompleted",
                table: "progresses",
                newName: "is_completed");

            migrationBuilder.RenameColumn(
                name: "ProgressId",
                table: "progresses",
                newName: "progress_id");

            migrationBuilder.RenameIndex(
                name: "IX_Progresses_StudentId",
                table: "progresses",
                newName: "IX_progresses_student_id");

            migrationBuilder.RenameIndex(
                name: "IX_Progresses_LessonId_StudentId",
                table: "progresses",
                newName: "IX_progresses_lesson_id_student_id");

            migrationBuilder.RenameColumn(
                name: "Title",
                table: "lessons",
                newName: "title");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "lessons",
                newName: "description");

            migrationBuilder.RenameColumn(
                name: "OrderIndex",
                table: "lessons",
                newName: "order_index");

            migrationBuilder.RenameColumn(
                name: "LessonType",
                table: "lessons",
                newName: "lesson_type");

            migrationBuilder.RenameColumn(
                name: "CourseId",
                table: "lessons",
                newName: "course_id");

            migrationBuilder.RenameColumn(
                name: "ContentUrl",
                table: "lessons",
                newName: "content_url");

            migrationBuilder.RenameColumn(
                name: "LessonId",
                table: "lessons",
                newName: "lesson_id");

            migrationBuilder.RenameIndex(
                name: "IX_Lessons_CourseId_OrderIndex",
                table: "lessons",
                newName: "IX_lessons_course_id_order_index");

            migrationBuilder.RenameColumn(
                name: "StudentId",
                table: "enrollments",
                newName: "student_id");

            migrationBuilder.RenameColumn(
                name: "EnrolledAt",
                table: "enrollments",
                newName: "enrolled_at");

            migrationBuilder.RenameColumn(
                name: "CourseId",
                table: "enrollments",
                newName: "course_id");

            migrationBuilder.RenameColumn(
                name: "EnrollmentId",
                table: "enrollments",
                newName: "enrollment_id");

            migrationBuilder.RenameIndex(
                name: "IX_Enrollments_StudentId",
                table: "enrollments",
                newName: "IX_enrollments_student_id");

            migrationBuilder.RenameIndex(
                name: "IX_Enrollments_CourseId_StudentId",
                table: "enrollments",
                newName: "IX_enrollments_course_id_student_id");

            migrationBuilder.RenameColumn(
                name: "Title",
                table: "courses",
                newName: "title");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "courses",
                newName: "description");

            migrationBuilder.RenameColumn(
                name: "Category",
                table: "courses",
                newName: "category");

            migrationBuilder.RenameColumn(
                name: "ThumbnailUrl",
                table: "courses",
                newName: "thumbnail_url");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                table: "courses",
                newName: "created_by");

            migrationBuilder.RenameColumn(
                name: "CategoryId",
                table: "courses",
                newName: "category_id");

            migrationBuilder.RenameColumn(
                name: "CourseId",
                table: "courses",
                newName: "course_id");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "courses",
                newName: "course_type");

            migrationBuilder.RenameIndex(
                name: "IX_Courses_Category",
                table: "courses",
                newName: "IX_courses_category");

            migrationBuilder.RenameIndex(
                name: "IX_Courses_CreatedBy",
                table: "courses",
                newName: "IX_courses_created_by");

            migrationBuilder.RenameIndex(
                name: "IX_Courses_CategoryId",
                table: "courses",
                newName: "IX_courses_category_id");

            migrationBuilder.RenameColumn(
                name: "Slug",
                table: "categories",
                newName: "slug");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "categories",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "categories",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "categories",
                newName: "is_active");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "categories",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "CategoryId",
                table: "categories",
                newName: "category_id");

            migrationBuilder.RenameIndex(
                name: "IX_Categories_Name",
                table: "categories",
                newName: "IX_categories_name");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "user_roles",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "RoleId",
                table: "user_roles",
                newName: "role_id");

            migrationBuilder.RenameColumn(
                name: "UserRoleId",
                table: "user_roles",
                newName: "user_role_id");

            migrationBuilder.RenameIndex(
                name: "IX_UserRoles_UserId_RoleId",
                table: "user_roles",
                newName: "IX_user_roles_user_id_role_id");

            migrationBuilder.RenameIndex(
                name: "IX_UserRoles_RoleId",
                table: "user_roles",
                newName: "IX_user_roles_role_id");

            migrationBuilder.AddColumn<int>(
                name: "current_page",
                table: "progresses",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "current_time_seconds",
                table: "progresses",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "lessons",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "total_duration_seconds",
                table: "lessons",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "total_pages",
                table: "lessons",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "lessons",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_mandatory",
                table: "enrollments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "learning_status",
                table: "enrollments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<float>(
                name: "progress_percent",
                table: "enrollments",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<int>(
                name: "course_status",
                table: "courses",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "end_date",
                table: "courses",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "registration_end_date",
                table: "courses",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "registration_start_date",
                table: "courses",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "start_date",
                table: "courses",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_users",
                table: "users",
                column: "user_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_roles",
                table: "roles",
                column: "role_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_progresses",
                table: "progresses",
                column: "progress_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_lessons",
                table: "lessons",
                column: "lesson_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_enrollments",
                table: "enrollments",
                column: "enrollment_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_courses",
                table: "courses",
                column: "course_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_categories",
                table: "categories",
                column: "category_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_user_roles",
                table: "user_roles",
                column: "user_role_id");

            migrationBuilder.CreateIndex(
                name: "IX_courses_course_status",
                table: "courses",
                column: "course_status");

            migrationBuilder.CreateIndex(
                name: "IX_courses_course_type",
                table: "courses",
                column: "course_type");

            migrationBuilder.CreateIndex(
                name: "IX_categories_slug",
                table: "categories",
                column: "slug",
                unique: true,
                filter: "[slug] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_courses_categories_category_id",
                table: "courses",
                column: "category_id",
                principalTable: "categories",
                principalColumn: "category_id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_courses_users_created_by",
                table: "courses",
                column: "created_by",
                principalTable: "users",
                principalColumn: "user_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_enrollments_courses_course_id",
                table: "enrollments",
                column: "course_id",
                principalTable: "courses",
                principalColumn: "course_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_enrollments_users_student_id",
                table: "enrollments",
                column: "student_id",
                principalTable: "users",
                principalColumn: "user_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_lessons_courses_course_id",
                table: "lessons",
                column: "course_id",
                principalTable: "courses",
                principalColumn: "course_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_progresses_lessons_lesson_id",
                table: "progresses",
                column: "lesson_id",
                principalTable: "lessons",
                principalColumn: "lesson_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_progresses_users_student_id",
                table: "progresses",
                column: "student_id",
                principalTable: "users",
                principalColumn: "user_id");

            migrationBuilder.AddForeignKey(
                name: "FK_user_roles_roles_role_id",
                table: "user_roles",
                column: "role_id",
                principalTable: "roles",
                principalColumn: "role_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_user_roles_users_user_id",
                table: "user_roles",
                column: "user_id",
                principalTable: "users",
                principalColumn: "user_id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_courses_categories_category_id",
                table: "courses");

            migrationBuilder.DropForeignKey(
                name: "FK_courses_users_created_by",
                table: "courses");

            migrationBuilder.DropForeignKey(
                name: "FK_enrollments_courses_course_id",
                table: "enrollments");

            migrationBuilder.DropForeignKey(
                name: "FK_enrollments_users_student_id",
                table: "enrollments");

            migrationBuilder.DropForeignKey(
                name: "FK_lessons_courses_course_id",
                table: "lessons");

            migrationBuilder.DropForeignKey(
                name: "FK_progresses_lessons_lesson_id",
                table: "progresses");

            migrationBuilder.DropForeignKey(
                name: "FK_progresses_users_student_id",
                table: "progresses");

            migrationBuilder.DropForeignKey(
                name: "FK_user_roles_roles_role_id",
                table: "user_roles");

            migrationBuilder.DropForeignKey(
                name: "FK_user_roles_users_user_id",
                table: "user_roles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_users",
                table: "users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_roles",
                table: "roles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_progresses",
                table: "progresses");

            migrationBuilder.DropPrimaryKey(
                name: "PK_lessons",
                table: "lessons");

            migrationBuilder.DropPrimaryKey(
                name: "PK_enrollments",
                table: "enrollments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_courses",
                table: "courses");

            migrationBuilder.DropIndex(
                name: "IX_courses_course_status",
                table: "courses");

            migrationBuilder.DropIndex(
                name: "IX_courses_course_type",
                table: "courses");

            migrationBuilder.DropPrimaryKey(
                name: "PK_categories",
                table: "categories");

            migrationBuilder.DropIndex(
                name: "IX_categories_slug",
                table: "categories");

            migrationBuilder.DropPrimaryKey(
                name: "PK_user_roles",
                table: "user_roles");

            migrationBuilder.DropColumn(
                name: "current_page",
                table: "progresses");

            migrationBuilder.DropColumn(
                name: "current_time_seconds",
                table: "progresses");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "lessons");

            migrationBuilder.DropColumn(
                name: "total_duration_seconds",
                table: "lessons");

            migrationBuilder.DropColumn(
                name: "total_pages",
                table: "lessons");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "lessons");

            migrationBuilder.DropColumn(
                name: "is_mandatory",
                table: "enrollments");

            migrationBuilder.DropColumn(
                name: "learning_status",
                table: "enrollments");

            migrationBuilder.DropColumn(
                name: "progress_percent",
                table: "enrollments");

            migrationBuilder.DropColumn(
                name: "course_status",
                table: "courses");

            migrationBuilder.DropColumn(
                name: "end_date",
                table: "courses");

            migrationBuilder.DropColumn(
                name: "registration_end_date",
                table: "courses");

            migrationBuilder.DropColumn(
                name: "registration_start_date",
                table: "courses");

            migrationBuilder.DropColumn(
                name: "start_date",
                table: "courses");

            migrationBuilder.RenameTable(
                name: "users",
                newName: "Users");

            migrationBuilder.RenameTable(
                name: "roles",
                newName: "Roles");

            migrationBuilder.RenameTable(
                name: "progresses",
                newName: "Progresses");

            migrationBuilder.RenameTable(
                name: "lessons",
                newName: "Lessons");

            migrationBuilder.RenameTable(
                name: "enrollments",
                newName: "Enrollments");

            migrationBuilder.RenameTable(
                name: "courses",
                newName: "Courses");

            migrationBuilder.RenameTable(
                name: "categories",
                newName: "Categories");

            migrationBuilder.RenameTable(
                name: "user_roles",
                newName: "UserRoles");

            migrationBuilder.RenameColumn(
                name: "email",
                table: "Users",
                newName: "Email");

            migrationBuilder.RenameColumn(
                name: "reset_token_expiry",
                table: "Users",
                newName: "ResetTokenExpiry");

            migrationBuilder.RenameColumn(
                name: "reset_token",
                table: "Users",
                newName: "ResetToken");

            migrationBuilder.RenameColumn(
                name: "password_hash",
                table: "Users",
                newName: "PasswordHash");

            migrationBuilder.RenameColumn(
                name: "is_active",
                table: "Users",
                newName: "IsActive");

            migrationBuilder.RenameColumn(
                name: "full_name",
                table: "Users",
                newName: "FullName");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "Users",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "Users",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_users_email",
                table: "Users",
                newName: "IX_Users_Email");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "Roles",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "role_id",
                table: "Roles",
                newName: "RoleId");

            migrationBuilder.RenameIndex(
                name: "IX_roles_name",
                table: "Roles",
                newName: "IX_Roles_Name");

            migrationBuilder.RenameColumn(
                name: "student_id",
                table: "Progresses",
                newName: "StudentId");

            migrationBuilder.RenameColumn(
                name: "lesson_id",
                table: "Progresses",
                newName: "LessonId");

            migrationBuilder.RenameColumn(
                name: "last_update",
                table: "Progresses",
                newName: "LastUpdate");

            migrationBuilder.RenameColumn(
                name: "is_completed",
                table: "Progresses",
                newName: "IsCompleted");

            migrationBuilder.RenameColumn(
                name: "progress_id",
                table: "Progresses",
                newName: "ProgressId");

            migrationBuilder.RenameIndex(
                name: "IX_progresses_student_id",
                table: "Progresses",
                newName: "IX_Progresses_StudentId");

            migrationBuilder.RenameIndex(
                name: "IX_progresses_lesson_id_student_id",
                table: "Progresses",
                newName: "IX_Progresses_LessonId_StudentId");

            migrationBuilder.RenameColumn(
                name: "title",
                table: "Lessons",
                newName: "Title");

            migrationBuilder.RenameColumn(
                name: "description",
                table: "Lessons",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "order_index",
                table: "Lessons",
                newName: "OrderIndex");

            migrationBuilder.RenameColumn(
                name: "lesson_type",
                table: "Lessons",
                newName: "LessonType");

            migrationBuilder.RenameColumn(
                name: "course_id",
                table: "Lessons",
                newName: "CourseId");

            migrationBuilder.RenameColumn(
                name: "content_url",
                table: "Lessons",
                newName: "ContentUrl");

            migrationBuilder.RenameColumn(
                name: "lesson_id",
                table: "Lessons",
                newName: "LessonId");

            migrationBuilder.RenameIndex(
                name: "IX_lessons_course_id_order_index",
                table: "Lessons",
                newName: "IX_Lessons_CourseId_OrderIndex");

            migrationBuilder.RenameColumn(
                name: "student_id",
                table: "Enrollments",
                newName: "StudentId");

            migrationBuilder.RenameColumn(
                name: "enrolled_at",
                table: "Enrollments",
                newName: "EnrolledAt");

            migrationBuilder.RenameColumn(
                name: "course_id",
                table: "Enrollments",
                newName: "CourseId");

            migrationBuilder.RenameColumn(
                name: "enrollment_id",
                table: "Enrollments",
                newName: "EnrollmentId");

            migrationBuilder.RenameIndex(
                name: "IX_enrollments_student_id",
                table: "Enrollments",
                newName: "IX_Enrollments_StudentId");

            migrationBuilder.RenameIndex(
                name: "IX_enrollments_course_id_student_id",
                table: "Enrollments",
                newName: "IX_Enrollments_CourseId_StudentId");

            migrationBuilder.RenameColumn(
                name: "title",
                table: "Courses",
                newName: "Title");

            migrationBuilder.RenameColumn(
                name: "description",
                table: "Courses",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "category",
                table: "Courses",
                newName: "Category");

            migrationBuilder.RenameColumn(
                name: "thumbnail_url",
                table: "Courses",
                newName: "ThumbnailUrl");

            migrationBuilder.RenameColumn(
                name: "created_by",
                table: "Courses",
                newName: "CreatedBy");

            migrationBuilder.RenameColumn(
                name: "category_id",
                table: "Courses",
                newName: "CategoryId");

            migrationBuilder.RenameColumn(
                name: "course_id",
                table: "Courses",
                newName: "CourseId");

            migrationBuilder.RenameColumn(
                name: "course_type",
                table: "Courses",
                newName: "Status");

            migrationBuilder.RenameIndex(
                name: "IX_courses_category",
                table: "Courses",
                newName: "IX_Courses_Category");

            migrationBuilder.RenameIndex(
                name: "IX_courses_created_by",
                table: "Courses",
                newName: "IX_Courses_CreatedBy");

            migrationBuilder.RenameIndex(
                name: "IX_courses_category_id",
                table: "Courses",
                newName: "IX_Courses_CategoryId");

            migrationBuilder.RenameColumn(
                name: "slug",
                table: "Categories",
                newName: "Slug");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "Categories",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "Categories",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "is_active",
                table: "Categories",
                newName: "IsActive");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "Categories",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "category_id",
                table: "Categories",
                newName: "CategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_categories_name",
                table: "Categories",
                newName: "IX_Categories_Name");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "UserRoles",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "role_id",
                table: "UserRoles",
                newName: "RoleId");

            migrationBuilder.RenameColumn(
                name: "user_role_id",
                table: "UserRoles",
                newName: "UserRoleId");

            migrationBuilder.RenameIndex(
                name: "IX_user_roles_user_id_role_id",
                table: "UserRoles",
                newName: "IX_UserRoles_UserId_RoleId");

            migrationBuilder.RenameIndex(
                name: "IX_user_roles_role_id",
                table: "UserRoles",
                newName: "IX_UserRoles_RoleId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Users",
                table: "Users",
                column: "UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Roles",
                table: "Roles",
                column: "RoleId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Progresses",
                table: "Progresses",
                column: "ProgressId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Lessons",
                table: "Lessons",
                column: "LessonId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Enrollments",
                table: "Enrollments",
                column: "EnrollmentId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Courses",
                table: "Courses",
                column: "CourseId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Categories",
                table: "Categories",
                column: "CategoryId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserRoles",
                table: "UserRoles",
                column: "UserRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_Slug",
                table: "Categories",
                column: "Slug",
                unique: true,
                filter: "[Slug] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Courses_Categories_CategoryId",
                table: "Courses",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "CategoryId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Courses_Users_CreatedBy",
                table: "Courses",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Enrollments_Courses_CourseId",
                table: "Enrollments",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "CourseId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Enrollments_Users_StudentId",
                table: "Enrollments",
                column: "StudentId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Lessons_Courses_CourseId",
                table: "Lessons",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "CourseId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Progresses_Lessons_LessonId",
                table: "Progresses",
                column: "LessonId",
                principalTable: "Lessons",
                principalColumn: "LessonId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Progresses_Users_StudentId",
                table: "Progresses",
                column: "StudentId",
                principalTable: "Users",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserRoles_Roles_RoleId",
                table: "UserRoles",
                column: "RoleId",
                principalTable: "Roles",
                principalColumn: "RoleId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserRoles_Users_UserId",
                table: "UserRoles",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
