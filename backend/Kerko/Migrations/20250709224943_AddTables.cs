using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kerko.Migrations
{
    /// <inheritdoc />
    public partial class AddTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Patronazhist",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    NumriPersonal = table.Column<string>(type: "TEXT", nullable: true),
                    Emri = table.Column<string>(type: "TEXT", nullable: true),
                    Mbiemri = table.Column<string>(type: "TEXT", nullable: true),
                    Atesi = table.Column<string>(type: "TEXT", nullable: true),
                    Datelindja = table.Column<string>(type: "TEXT", nullable: true),
                    QV = table.Column<string>(type: "TEXT", nullable: true),
                    ListaNr = table.Column<string>(type: "TEXT", nullable: true),
                    Tel = table.Column<string>(type: "TEXT", nullable: true),
                    Emigrant = table.Column<string>(type: "TEXT", nullable: true),
                    Country = table.Column<string>(type: "TEXT", nullable: true),
                    ISigurte = table.Column<string>(type: "TEXT", nullable: true),
                    Koment = table.Column<string>(type: "TEXT", nullable: true),
                    Patronazhisti = table.Column<string>(type: "TEXT", nullable: true),
                    Preferenca = table.Column<string>(type: "TEXT", nullable: true),
                    Census2013Preferenca = table.Column<string>(type: "TEXT", nullable: true),
                    Census2013Siguria = table.Column<string>(type: "TEXT", nullable: true),
                    Vendlindja = table.Column<string>(type: "TEXT", nullable: true),
                    Kompania = table.Column<string>(type: "TEXT", nullable: true),
                    KodBanese = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Patronazhist", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Person",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Adresa = table.Column<string>(type: "TEXT", nullable: true),
                    NrBaneses = table.Column<string>(type: "TEXT", nullable: true),
                    Emer = table.Column<string>(type: "TEXT", nullable: true),
                    Mbiemer = table.Column<string>(type: "TEXT", nullable: true),
                    Atesi = table.Column<string>(type: "TEXT", nullable: true),
                    Amesi = table.Column<string>(type: "TEXT", nullable: true),
                    Datelindja = table.Column<string>(type: "TEXT", nullable: true),
                    Vendlindja = table.Column<string>(type: "TEXT", nullable: true),
                    Seksi = table.Column<string>(type: "TEXT", nullable: true),
                    LidhjaMeKryefamiljarin = table.Column<string>(type: "TEXT", nullable: true),
                    Qyteti = table.Column<string>(type: "TEXT", nullable: true),
                    GjendjeCivile = table.Column<string>(type: "TEXT", nullable: true),
                    Kombesia = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Person", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Rrogat",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    NumriPersonal = table.Column<string>(type: "TEXT", nullable: true),
                    Emri = table.Column<string>(type: "TEXT", nullable: true),
                    Mbiemri = table.Column<string>(type: "TEXT", nullable: true),
                    NIPT = table.Column<string>(type: "TEXT", nullable: true),
                    DRT = table.Column<string>(type: "TEXT", nullable: true),
                    PagaBruto = table.Column<int>(type: "INTEGER", nullable: true),
                    Profesioni = table.Column<string>(type: "TEXT", nullable: true),
                    Kategoria = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rrogat", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SearchLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    IpAddress = table.Column<string>(type: "TEXT", nullable: false),
                    SearchType = table.Column<string>(type: "TEXT", nullable: false),
                    SearchParams = table.Column<string>(type: "TEXT", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false),
                    WasSuccessful = table.Column<bool>(type: "INTEGER", nullable: false),
                    ErrorMessage = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SearchLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Targat",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    NumriTarges = table.Column<string>(type: "TEXT", nullable: true),
                    Marka = table.Column<string>(type: "TEXT", nullable: true),
                    Modeli = table.Column<string>(type: "TEXT", nullable: true),
                    Ngjyra = table.Column<string>(type: "TEXT", nullable: true),
                    NumriPersonal = table.Column<string>(type: "TEXT", nullable: true),
                    Emri = table.Column<string>(type: "TEXT", nullable: true),
                    Mbiemri = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Targat", x => x.Id);
                });
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
