using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Online_Course.Migrations
{
    /// <inheritdoc />
    public partial class AddTypeLesson_UpdateContentUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "VideoUrl",
                table: "Lessons",
                newName: "ContentUrl");

            migrationBuilder.AddColumn<int>(
                name: "LessonType",
                table: "Lessons",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LessonType",
                table: "Lessons");

            migrationBuilder.RenameColumn(
                name: "ContentUrl",
                table: "Lessons",
                newName: "VideoUrl");
        }
    }
}
