using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Kerko.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Patronazhist",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NumriPersonal = table.Column<string>(type: "text", nullable: true),
                    Emri = table.Column<string>(type: "text", nullable: true),
                    Mbiemri = table.Column<string>(type: "text", nullable: true),
                    Atesi = table.Column<string>(type: "text", nullable: true),
                    Datelindja = table.Column<string>(type: "text", nullable: true),
                    QV = table.Column<string>(type: "text", nullable: true),
                    ListaNr = table.Column<string>(type: "text", nullable: true),
                    Tel = table.Column<string>(type: "text", nullable: true),
                    Emigrant = table.Column<string>(type: "text", nullable: true),
                    Country = table.Column<string>(type: "text", nullable: true),
                    ISigurte = table.Column<string>(type: "text", nullable: true),
                    Koment = table.Column<string>(type: "text", nullable: true),
                    Patronazhisti = table.Column<string>(type: "text", nullable: true),
                    Preferenca = table.Column<string>(type: "text", nullable: true),
                    Census2013Preferenca = table.Column<string>(type: "text", nullable: true),
                    Census2013Siguria = table.Column<string>(type: "text", nullable: true),
                    Vendlindja = table.Column<string>(type: "text", nullable: true),
                    Kompania = table.Column<string>(type: "text", nullable: true),
                    KodBanese = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Patronazhist", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Person",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Adresa = table.Column<string>(type: "text", nullable: true),
                    NrBaneses = table.Column<string>(type: "text", nullable: true),
                    Emer = table.Column<string>(type: "text", nullable: true),
                    Mbiemer = table.Column<string>(type: "text", nullable: true),
                    Atesi = table.Column<string>(type: "text", nullable: true),
                    Amesi = table.Column<string>(type: "text", nullable: true),
                    Datelindja = table.Column<string>(type: "text", nullable: true),
                    Vendlindja = table.Column<string>(type: "text", nullable: true),
                    Seksi = table.Column<string>(type: "text", nullable: true),
                    LidhjaMeKryefamiljarin = table.Column<string>(type: "text", nullable: true),
                    Qyteti = table.Column<string>(type: "text", nullable: true),
                    GjendjeCivile = table.Column<string>(type: "text", nullable: true),
                    Kombesia = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Person", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Rrogat",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NumriPersonal = table.Column<string>(type: "text", nullable: true),
                    Emri = table.Column<string>(type: "text", nullable: true),
                    Mbiemri = table.Column<string>(type: "text", nullable: true),
                    NIPT = table.Column<string>(type: "text", nullable: true),
                    DRT = table.Column<string>(type: "text", nullable: true),
                    PagaBruto = table.Column<int>(type: "integer", nullable: true),
                    Profesioni = table.Column<string>(type: "text", nullable: true),
                    Kategoria = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rrogat", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SearchLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IpAddress = table.Column<string>(type: "text", nullable: false),
                    SearchType = table.Column<string>(type: "text", nullable: false),
                    SearchParams = table.Column<string>(type: "text", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    WasSuccessful = table.Column<bool>(type: "boolean", nullable: false),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SearchLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Targat",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NumriTarges = table.Column<string>(type: "text", nullable: true),
                    Marka = table.Column<string>(type: "text", nullable: true),
                    Modeli = table.Column<string>(type: "text", nullable: true),
                    Ngjyra = table.Column<string>(type: "text", nullable: true),
                    NumriPersonal = table.Column<string>(type: "text", nullable: true),
                    Emri = table.Column<string>(type: "text", nullable: true),
                    Mbiemri = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Targat", x => x.Id);
                });

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
                name: "IX_SearchLogs_IpAddress",
                table: "SearchLogs",
                column: "IpAddress");

            migrationBuilder.CreateIndex(
                name: "IX_SearchLogs_Timestamp",
                table: "SearchLogs",
                column: "Timestamp");

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Patronazhist");

            migrationBuilder.DropTable(
                name: "Person");

            migrationBuilder.DropTable(
                name: "Rrogat");

            migrationBuilder.DropTable(
                name: "SearchLogs");

            migrationBuilder.DropTable(
                name: "Targat");
        }
    }
}
