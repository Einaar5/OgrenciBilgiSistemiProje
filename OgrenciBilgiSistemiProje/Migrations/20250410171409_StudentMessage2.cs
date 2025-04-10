using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OgrenciBilgiSistemiProje.Migrations
{
    /// <inheritdoc />
    public partial class StudentMessage2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StudentMessages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    SentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    SenderStudentId = table.Column<int>(type: "int", nullable: false),
                    ReceiverTeacherId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentMessages_Students_SenderStudentId",
                        column: x => x.SenderStudentId,
                        principalTable: "Students",
                        principalColumn: "StudentId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StudentMessages_Teachers_ReceiverTeacherId",
                        column: x => x.ReceiverTeacherId,
                        principalTable: "Teachers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StudentMessages_ReceiverTeacherId",
                table: "StudentMessages",
                column: "ReceiverTeacherId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentMessages_SenderStudentId",
                table: "StudentMessages",
                column: "SenderStudentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StudentMessages");
        }
    }
}
