using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kerko.Migrations
{
    /// <inheritdoc />
    public partial class indexEmerMbiemer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Targat_Mbiemri_Emri",
                table: "Targat",
                columns: new[] { "Mbiemri", "Emri" });

            migrationBuilder.CreateIndex(
                name: "IX_Rrogat_Mbiemri_Emri",
                table: "Rrogat",
                columns: new[] { "Mbiemri", "Emri" });

            migrationBuilder.CreateIndex(
                name: "IX_Person_Mbiemer_Emer",
                table: "Person",
                columns: new[] { "Mbiemer", "Emer" });

            migrationBuilder.CreateIndex(
                name: "IX_Patronazhist_Mbiemri_Emri",
                table: "Patronazhist",
                columns: new[] { "Mbiemri", "Emri" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Targat_Mbiemri_Emri",
                table: "Targat");

            migrationBuilder.DropIndex(
                name: "IX_Rrogat_Mbiemri_Emri",
                table: "Rrogat");

            migrationBuilder.DropIndex(
                name: "IX_Person_Mbiemer_Emer",
                table: "Person");

            migrationBuilder.DropIndex(
                name: "IX_Patronazhist_Mbiemri_Emri",
                table: "Patronazhist");
        }
    }
}
