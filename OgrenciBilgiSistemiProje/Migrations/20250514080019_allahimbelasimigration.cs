using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OgrenciBilgiSistemiProje.Migrations
{
    /// <inheritdoc />
    public partial class allahimbelasimigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Grades_Quizzes_QuizId",
                table: "Grades");

            migrationBuilder.DropIndex(
                name: "IX_Grades_StudentId_LessonId",
                table: "Grades");

            migrationBuilder.CreateIndex(
                name: "IX_Grades_StudentId_QuizId",
                table: "Grades",
                columns: new[] { "StudentId", "QuizId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Grades_Quizzes_QuizId",
                table: "Grades",
                column: "QuizId",
                principalTable: "Quizzes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Grades_Quizzes_QuizId",
                table: "Grades");

            migrationBuilder.DropIndex(
                name: "IX_Grades_StudentId_QuizId",
                table: "Grades");

            migrationBuilder.CreateIndex(
                name: "IX_Grades_StudentId_LessonId",
                table: "Grades",
                columns: new[] { "StudentId", "LessonId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Grades_Quizzes_QuizId",
                table: "Grades",
                column: "QuizId",
                principalTable: "Quizzes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
