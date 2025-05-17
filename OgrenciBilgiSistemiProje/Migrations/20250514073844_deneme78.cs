using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OgrenciBilgiSistemiProje.Migrations
{
    /// <inheritdoc />
    public partial class deneme78 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Quizzes_Lessons_lessonId",
                table: "Quizzes");

            migrationBuilder.RenameColumn(
                name: "lessonId",
                table: "Quizzes",
                newName: "LessonId");

            migrationBuilder.RenameIndex(
                name: "IX_Quizzes_lessonId",
                table: "Quizzes",
                newName: "IX_Quizzes_LessonId");

            migrationBuilder.AddForeignKey(
                name: "FK_Quizzes_Lessons_LessonId",
                table: "Quizzes",
                column: "LessonId",
                principalTable: "Lessons",
                principalColumn: "LessonId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Quizzes_Lessons_LessonId",
                table: "Quizzes");

            migrationBuilder.RenameColumn(
                name: "LessonId",
                table: "Quizzes",
                newName: "lessonId");

            migrationBuilder.RenameIndex(
                name: "IX_Quizzes_LessonId",
                table: "Quizzes",
                newName: "IX_Quizzes_lessonId");

            migrationBuilder.AddForeignKey(
                name: "FK_Quizzes_Lessons_lessonId",
                table: "Quizzes",
                column: "lessonId",
                principalTable: "Lessons",
                principalColumn: "LessonId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
