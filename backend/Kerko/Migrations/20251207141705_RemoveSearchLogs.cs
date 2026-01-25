using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kerko.Migrations
{
    /// <inheritdoc />
    public partial class RemoveSearchLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SearchLogs");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SearchLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ErrorMessage = table.Column<string>(type: "TEXT", nullable: true),
                    IpAddress = table.Column<string>(type: "TEXT", nullable: false),
                    SearchParams = table.Column<string>(type: "TEXT", nullable: false),
                    SearchType = table.Column<string>(type: "TEXT", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false),
                    WasSuccessful = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SearchLogs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SearchLogs_IpAddress",
                table: "SearchLogs",
                column: "IpAddress");

            migrationBuilder.CreateIndex(
                name: "IX_SearchLogs_Timestamp",
                table: "SearchLogs",
                column: "Timestamp");
        }
    }
}
