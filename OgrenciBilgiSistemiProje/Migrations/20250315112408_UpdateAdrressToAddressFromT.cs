using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OgrenciBilgiSistemiProje.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAdrressToAddressFromT : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TeacherAdrress",
                table: "Teachers",
                newName: "TeacherAddress");

            migrationBuilder.RenameColumn(
                name: "ImgFileName",
                table: "Teachers",
                newName: "ImageFileName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TeacherAddress",
                table: "Teachers",
                newName: "TeacherAdrress");

            migrationBuilder.RenameColumn(
                name: "ImageFileName",
                table: "Teachers",
                newName: "ImgFileName");
        }
    }
}
