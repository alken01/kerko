using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kerko.Migrations
{
    /// <inheritdoc />
    public partial class AddEmriNormalizedIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Targat_Emri",
                table: "Targat");

            migrationBuilder.DropIndex(
                name: "IX_Targat_Mbiemri",
                table: "Targat");

            migrationBuilder.DropIndex(
                name: "IX_Rrogat_Emri",
                table: "Rrogat");

            migrationBuilder.DropIndex(
                name: "IX_Rrogat_Mbiemri",
                table: "Rrogat");

            migrationBuilder.DropIndex(
                name: "IX_Person_Emer",
                table: "Person");

            migrationBuilder.DropIndex(
                name: "IX_Person_Mbiemer",
                table: "Person");

            migrationBuilder.DropIndex(
                name: "IX_Patronazhist_Emri",
                table: "Patronazhist");

            migrationBuilder.DropIndex(
                name: "IX_Patronazhist_Mbiemri",
                table: "Patronazhist");

            migrationBuilder.CreateIndex(
                name: "IX_Targat_EmriNormalized",
                table: "Targat",
                column: "EmriNormalized");

            migrationBuilder.CreateIndex(
                name: "IX_Rrogat_EmriNormalized",
                table: "Rrogat",
                column: "EmriNormalized");

            migrationBuilder.CreateIndex(
                name: "IX_Person_EmerNormalized",
                table: "Person",
                column: "EmerNormalized");

            migrationBuilder.CreateIndex(
                name: "IX_Patronazhist_EmriNormalized",
                table: "Patronazhist",
                column: "EmriNormalized");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Targat_EmriNormalized",
                table: "Targat");

            migrationBuilder.DropIndex(
                name: "IX_Rrogat_EmriNormalized",
                table: "Rrogat");

            migrationBuilder.DropIndex(
                name: "IX_Person_EmerNormalized",
                table: "Person");

            migrationBuilder.DropIndex(
                name: "IX_Patronazhist_EmriNormalized",
                table: "Patronazhist");

            migrationBuilder.CreateIndex(
                name: "IX_Targat_Emri",
                table: "Targat",
                column: "Emri");

            migrationBuilder.CreateIndex(
                name: "IX_Targat_Mbiemri",
                table: "Targat",
                column: "Mbiemri");

            migrationBuilder.CreateIndex(
                name: "IX_Rrogat_Emri",
                table: "Rrogat",
                column: "Emri");

            migrationBuilder.CreateIndex(
                name: "IX_Rrogat_Mbiemri",
                table: "Rrogat",
                column: "Mbiemri");

            migrationBuilder.CreateIndex(
                name: "IX_Person_Emer",
                table: "Person",
                column: "Emer");

            migrationBuilder.CreateIndex(
                name: "IX_Person_Mbiemer",
                table: "Person",
                column: "Mbiemer");

            migrationBuilder.CreateIndex(
                name: "IX_Patronazhist_Emri",
                table: "Patronazhist",
                column: "Emri");

            migrationBuilder.CreateIndex(
                name: "IX_Patronazhist_Mbiemri",
                table: "Patronazhist",
                column: "Mbiemri");
        }
    }
}
