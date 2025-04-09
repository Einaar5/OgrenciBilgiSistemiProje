using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OgrenciBilgiSistemiProje.Migrations
{
    /// <inheritdoc />
    public partial class Mig3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Grades_StudentId",
                table: "Grades");

            migrationBuilder.CreateIndex(
                name: "IX_Teachers_TeacherMail",
                table: "Teachers",
                column: "TeacherMail",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Students_StudentEmail",
                table: "Students",
                column: "StudentEmail",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Grades_StudentId_LessonId",
                table: "Grades",
                columns: new[] { "StudentId", "LessonId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Teachers_TeacherMail",
                table: "Teachers");

            migrationBuilder.DropIndex(
                name: "IX_Students_StudentEmail",
                table: "Students");

            migrationBuilder.DropIndex(
                name: "IX_Grades_StudentId_LessonId",
                table: "Grades");

            migrationBuilder.CreateIndex(
                name: "IX_Grades_StudentId",
                table: "Grades",
                column: "StudentId");
        }
    }
}
