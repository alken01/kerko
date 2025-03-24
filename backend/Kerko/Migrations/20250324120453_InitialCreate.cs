using Microsoft.EntityFrameworkCore.Migrations;

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
                name: "Patronazhists",
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
                    table.PrimaryKey("PK_Patronazhists", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Persons",
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
                    table.PrimaryKey("PK_Persons", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Rrogats",
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
                    table.PrimaryKey("PK_Rrogats", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Targats",
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
                    table.PrimaryKey("PK_Targats", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Patronazhists_Emri",
                table: "Patronazhists",
                column: "Emri");

            migrationBuilder.CreateIndex(
                name: "IX_Patronazhists_Mbiemri",
                table: "Patronazhists",
                column: "Mbiemri");

            migrationBuilder.CreateIndex(
                name: "IX_Persons_Emer",
                table: "Persons",
                column: "Emer");

            migrationBuilder.CreateIndex(
                name: "IX_Persons_Mbiemer",
                table: "Persons",
                column: "Mbiemer");

            migrationBuilder.CreateIndex(
                name: "IX_Rrogats_Emri",
                table: "Rrogats",
                column: "Emri");

            migrationBuilder.CreateIndex(
                name: "IX_Rrogats_Mbiemri",
                table: "Rrogats",
                column: "Mbiemri");

            migrationBuilder.CreateIndex(
                name: "IX_Targats_Emri",
                table: "Targats",
                column: "Emri");

            migrationBuilder.CreateIndex(
                name: "IX_Targats_Mbiemri",
                table: "Targats",
                column: "Mbiemri");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Patronazhists");

            migrationBuilder.DropTable(
                name: "Persons");

            migrationBuilder.DropTable(
                name: "Rrogats");

            migrationBuilder.DropTable(
                name: "Targats");
        }
    }
}
