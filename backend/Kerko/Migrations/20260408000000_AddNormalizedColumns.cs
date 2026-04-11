using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kerko.Migrations
{
    /// <inheritdoc />
    public partial class AddNormalizedColumns : Migration
    {
        // Diacritic-folding expression: LOWER + replace Ç/ç → c and Ë/ë → e.
        // SQLite's LOWER() only lowercases ASCII, so we must replace both cases
        // of the unicode letters explicitly.
        private static string NormalizeExpr(string column) =>
            $"REPLACE(REPLACE(REPLACE(REPLACE(LOWER(\"{column}\"), 'Ç', 'c'), 'ç', 'c'), 'Ë', 'e'), 'ë', 'e')";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // SQLite only permits VIRTUAL generated columns via ALTER TABLE ADD COLUMN.
            // The expression is evaluated on read (and on index update), so pairing
            // these with a B-tree index on the normalized columns is what makes the
            // name search fast: the index stores the already-folded values and can
            // be range-scanned directly, with no per-row REPLACE/LOWER work.

            migrationBuilder.Sql(
                $"ALTER TABLE \"Person\" ADD COLUMN \"EmerNormalized\" TEXT " +
                $"GENERATED ALWAYS AS ({NormalizeExpr("Emer")}) VIRTUAL;");
            migrationBuilder.Sql(
                $"ALTER TABLE \"Person\" ADD COLUMN \"MbiemerNormalized\" TEXT " +
                $"GENERATED ALWAYS AS ({NormalizeExpr("Mbiemer")}) VIRTUAL;");

            migrationBuilder.Sql(
                $"ALTER TABLE \"Rrogat\" ADD COLUMN \"EmriNormalized\" TEXT " +
                $"GENERATED ALWAYS AS ({NormalizeExpr("Emri")}) VIRTUAL;");
            migrationBuilder.Sql(
                $"ALTER TABLE \"Rrogat\" ADD COLUMN \"MbiemriNormalized\" TEXT " +
                $"GENERATED ALWAYS AS ({NormalizeExpr("Mbiemri")}) VIRTUAL;");

            migrationBuilder.Sql(
                $"ALTER TABLE \"Targat\" ADD COLUMN \"EmriNormalized\" TEXT " +
                $"GENERATED ALWAYS AS ({NormalizeExpr("Emri")}) VIRTUAL;");
            migrationBuilder.Sql(
                $"ALTER TABLE \"Targat\" ADD COLUMN \"MbiemriNormalized\" TEXT " +
                $"GENERATED ALWAYS AS ({NormalizeExpr("Mbiemri")}) VIRTUAL;");

            migrationBuilder.Sql(
                $"ALTER TABLE \"Patronazhist\" ADD COLUMN \"EmriNormalized\" TEXT " +
                $"GENERATED ALWAYS AS ({NormalizeExpr("Emri")}) VIRTUAL;");
            migrationBuilder.Sql(
                $"ALTER TABLE \"Patronazhist\" ADD COLUMN \"MbiemriNormalized\" TEXT " +
                $"GENERATED ALWAYS AS ({NormalizeExpr("Mbiemri")}) VIRTUAL;");

            // Drop the old unused composite indexes on the raw (unfolded) columns —
            // the search query no longer touches those columns, so the old composites
            // are dead weight. The single-column indexes stay because they may be
            // used by other code paths.
            migrationBuilder.DropIndex(name: "IX_Person_Mbiemer_Emer", table: "Person");
            migrationBuilder.DropIndex(name: "IX_Rrogat_Mbiemri_Emri", table: "Rrogat");
            migrationBuilder.DropIndex(name: "IX_Targat_Mbiemri_Emri", table: "Targat");
            migrationBuilder.DropIndex(name: "IX_Patronazhist_Mbiemri_Emri", table: "Patronazhist");

            // New composite indexes on the normalized columns: these are what the
            // KerkoAsync search actually uses. Column order (Mbiemri, Emri) matches
            // the WHERE clause so SQLite can seek the leading column and range-scan
            // the trailing one.
            migrationBuilder.CreateIndex(
                name: "IX_Person_MbiemerNormalized_EmerNormalized",
                table: "Person",
                columns: new[] { "MbiemerNormalized", "EmerNormalized" });

            migrationBuilder.CreateIndex(
                name: "IX_Rrogat_MbiemriNormalized_EmriNormalized",
                table: "Rrogat",
                columns: new[] { "MbiemriNormalized", "EmriNormalized" });

            migrationBuilder.CreateIndex(
                name: "IX_Targat_MbiemriNormalized_EmriNormalized",
                table: "Targat",
                columns: new[] { "MbiemriNormalized", "EmriNormalized" });

            migrationBuilder.CreateIndex(
                name: "IX_Patronazhist_MbiemriNormalized_EmriNormalized",
                table: "Patronazhist",
                columns: new[] { "MbiemriNormalized", "EmriNormalized" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Person_MbiemerNormalized_EmerNormalized",
                table: "Person");
            migrationBuilder.DropIndex(
                name: "IX_Rrogat_MbiemriNormalized_EmriNormalized",
                table: "Rrogat");
            migrationBuilder.DropIndex(
                name: "IX_Targat_MbiemriNormalized_EmriNormalized",
                table: "Targat");
            migrationBuilder.DropIndex(
                name: "IX_Patronazhist_MbiemriNormalized_EmriNormalized",
                table: "Patronazhist");

            migrationBuilder.DropColumn(name: "EmerNormalized", table: "Person");
            migrationBuilder.DropColumn(name: "MbiemerNormalized", table: "Person");
            migrationBuilder.DropColumn(name: "EmriNormalized", table: "Rrogat");
            migrationBuilder.DropColumn(name: "MbiemriNormalized", table: "Rrogat");
            migrationBuilder.DropColumn(name: "EmriNormalized", table: "Targat");
            migrationBuilder.DropColumn(name: "MbiemriNormalized", table: "Targat");
            migrationBuilder.DropColumn(name: "EmriNormalized", table: "Patronazhist");
            migrationBuilder.DropColumn(name: "MbiemriNormalized", table: "Patronazhist");

            migrationBuilder.CreateIndex(
                name: "IX_Person_Mbiemer_Emer",
                table: "Person",
                columns: new[] { "Mbiemer", "Emer" });
            migrationBuilder.CreateIndex(
                name: "IX_Rrogat_Mbiemri_Emri",
                table: "Rrogat",
                columns: new[] { "Mbiemri", "Emri" });
            migrationBuilder.CreateIndex(
                name: "IX_Targat_Mbiemri_Emri",
                table: "Targat",
                columns: new[] { "Mbiemri", "Emri" });
            migrationBuilder.CreateIndex(
                name: "IX_Patronazhist_Mbiemri_Emri",
                table: "Patronazhist",
                columns: new[] { "Mbiemri", "Emri" });
        }
    }
}
