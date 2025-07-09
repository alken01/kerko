using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kerko.Migrations
{
    /// <inheritdoc />
    public partial class AddIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Targat_Emri",
                table: "Targat",
                column: "Emri");

            migrationBuilder.CreateIndex(
                name: "IX_Targat_Mbiemri",
                table: "Targat",
                column: "Mbiemri");

            migrationBuilder.CreateIndex(
                name: "IX_Targat_Mbiemri_Emri",
                table: "Targat",
                columns: new[] { "Mbiemri", "Emri" });

            migrationBuilder.CreateIndex(
                name: "IX_Targat_NumriTarges",
                table: "Targat",
                column: "NumriTarges");

            migrationBuilder.CreateIndex(
                name: "IX_SearchLogs_IpAddress",
                table: "SearchLogs",
                column: "IpAddress");

            migrationBuilder.CreateIndex(
                name: "IX_SearchLogs_Timestamp",
                table: "SearchLogs",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_Rrogat_Emri",
                table: "Rrogat",
                column: "Emri");

            migrationBuilder.CreateIndex(
                name: "IX_Rrogat_Mbiemri",
                table: "Rrogat",
                column: "Mbiemri");

            migrationBuilder.CreateIndex(
                name: "IX_Rrogat_Mbiemri_Emri",
                table: "Rrogat",
                columns: new[] { "Mbiemri", "Emri" });

            migrationBuilder.CreateIndex(
                name: "IX_Person_Emer",
                table: "Person",
                column: "Emer");

            migrationBuilder.CreateIndex(
                name: "IX_Person_Mbiemer",
                table: "Person",
                column: "Mbiemer");

            migrationBuilder.CreateIndex(
                name: "IX_Person_Mbiemer_Emer",
                table: "Person",
                columns: new[] { "Mbiemer", "Emer" });

            migrationBuilder.CreateIndex(
                name: "IX_Patronazhist_Emri",
                table: "Patronazhist",
                column: "Emri");

            migrationBuilder.CreateIndex(
                name: "IX_Patronazhist_Mbiemri",
                table: "Patronazhist",
                column: "Mbiemri");

            migrationBuilder.CreateIndex(
                name: "IX_Patronazhist_Mbiemri_Emri",
                table: "Patronazhist",
                columns: new[] { "Mbiemri", "Emri" });

            migrationBuilder.CreateIndex(
                name: "IX_Patronazhist_Tel",
                table: "Patronazhist",
                column: "Tel");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Targat_Emri",
                table: "Targat");

            migrationBuilder.DropIndex(
                name: "IX_Targat_Mbiemri",
                table: "Targat");

            migrationBuilder.DropIndex(
                name: "IX_Targat_Mbiemri_Emri",
                table: "Targat");

            migrationBuilder.DropIndex(
                name: "IX_Targat_NumriTarges",
                table: "Targat");

            migrationBuilder.DropIndex(
                name: "IX_SearchLogs_IpAddress",
                table: "SearchLogs");

            migrationBuilder.DropIndex(
                name: "IX_SearchLogs_Timestamp",
                table: "SearchLogs");

            migrationBuilder.DropIndex(
                name: "IX_Rrogat_Emri",
                table: "Rrogat");

            migrationBuilder.DropIndex(
                name: "IX_Rrogat_Mbiemri",
                table: "Rrogat");

            migrationBuilder.DropIndex(
                name: "IX_Rrogat_Mbiemri_Emri",
                table: "Rrogat");

            migrationBuilder.DropIndex(
                name: "IX_Person_Emer",
                table: "Person");

            migrationBuilder.DropIndex(
                name: "IX_Person_Mbiemer",
                table: "Person");

            migrationBuilder.DropIndex(
                name: "IX_Person_Mbiemer_Emer",
                table: "Person");

            migrationBuilder.DropIndex(
                name: "IX_Patronazhist_Emri",
                table: "Patronazhist");

            migrationBuilder.DropIndex(
                name: "IX_Patronazhist_Mbiemri",
                table: "Patronazhist");

            migrationBuilder.DropIndex(
                name: "IX_Patronazhist_Mbiemri_Emri",
                table: "Patronazhist");

            migrationBuilder.DropIndex(
                name: "IX_Patronazhist_Tel",
                table: "Patronazhist");
        }
    }
}
