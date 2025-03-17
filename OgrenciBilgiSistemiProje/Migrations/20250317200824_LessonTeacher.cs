using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OgrenciBilgiSistemiProje.Migrations
{
    /// <inheritdoc />
    public partial class LessonTeacher : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LessonId",
                table: "Teachers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "LessonId1",
                table: "Teachers",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Teachers_LessonId1",
                table: "Teachers",
                column: "LessonId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Teachers_Lessons_LessonId1",
                table: "Teachers",
                column: "LessonId1",
                principalTable: "Lessons",
                principalColumn: "LessonId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Teachers_Lessons_LessonId1",
                table: "Teachers");

            migrationBuilder.DropIndex(
                name: "IX_Teachers_LessonId1",
                table: "Teachers");

            migrationBuilder.DropColumn(
                name: "LessonId",
                table: "Teachers");

            migrationBuilder.DropColumn(
                name: "LessonId1",
                table: "Teachers");
        }
    }
}
