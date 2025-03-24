using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kerko.Migrations
{
    /// <inheritdoc />
    public partial class indexNumriTarges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Targat_NumriTarges",
                table: "Targat",
                column: "NumriTarges");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Targat_NumriTarges",
                table: "Targat");
        }
    }
}
