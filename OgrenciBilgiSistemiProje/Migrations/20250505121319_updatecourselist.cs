using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OgrenciBilgiSistemiProje.Migrations
{
    /// <inheritdoc />
    public partial class updatecourselist : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "StudentLessonId",
                table: "CourseList",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CourseList_StudentLessonId",
                table: "CourseList",
                column: "StudentLessonId");

            migrationBuilder.AddForeignKey(
                name: "FK_CourseList_StudentLessons_StudentLessonId",
                table: "CourseList",
                column: "StudentLessonId",
                principalTable: "StudentLessons",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CourseList_StudentLessons_StudentLessonId",
                table: "CourseList");

            migrationBuilder.DropIndex(
                name: "IX_CourseList_StudentLessonId",
                table: "CourseList");

            migrationBuilder.DropColumn(
                name: "StudentLessonId",
                table: "CourseList");
        }
    }
}
