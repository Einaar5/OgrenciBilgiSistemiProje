using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OgrenciBilgiSistemiProje.Migrations
{
    /// <inheritdoc />
    public partial class Attendance3hour : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsCome",
                table: "Attendance",
                newName: "IsComeHour3");

            migrationBuilder.AddColumn<bool>(
                name: "IsComeHour1",
                table: "Attendance",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsComeHour2",
                table: "Attendance",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsComeHour1",
                table: "Attendance");

            migrationBuilder.DropColumn(
                name: "IsComeHour2",
                table: "Attendance");

            migrationBuilder.RenameColumn(
                name: "IsComeHour3",
                table: "Attendance",
                newName: "IsCome");
        }
    }
}
