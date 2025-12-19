using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Online_Course.Migrations
{
    /// <inheritdoc />
    public partial class RemoveCategoryStringColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_courses_category",
                table: "courses");

            migrationBuilder.DropColumn(
                name: "category",
                table: "courses");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "category",
                table: "courses",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_courses_category",
                table: "courses",
                column: "category");
        }
    }
}
