using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OgrenciBilgiSistemiProje.Migrations
{
    /// <inheritdoc />
    public partial class addQuizRel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "QuizId",
                table: "Grades",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Grades_QuizId",
                table: "Grades",
                column: "QuizId");

            migrationBuilder.AddForeignKey(
                name: "FK_Grades_Quizzes_QuizId",
                table: "Grades",
                column: "QuizId",
                principalTable: "Quizzes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Grades_Quizzes_QuizId",
                table: "Grades");

            migrationBuilder.DropIndex(
                name: "IX_Grades_QuizId",
                table: "Grades");

            migrationBuilder.DropColumn(
                name: "QuizId",
                table: "Grades");
        }
    }
}
