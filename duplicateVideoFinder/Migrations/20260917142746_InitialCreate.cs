using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace duplicateVideoFinder.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Entries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DirectoryPath = table.Column<string>(type: "TEXT", nullable: true),
                    GeneratorId = table.Column<string>(type: "TEXT", nullable: true),
                    FilePath = table.Column<string>(type: "TEXT", nullable: true),
                    FileLength = table.Column<long>(type: "INTEGER", nullable: false),
                    LastWriteUtcTicks = table.Column<long>(type: "INTEGER", nullable: false),
                    MetricType = table.Column<string>(type: "TEXT", nullable: true),
                    MetricJson = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Entries", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Entries_DirectoryPath_GeneratorId_FilePath",
                table: "Entries",
                columns: new[] { "DirectoryPath", "GeneratorId", "FilePath" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Entries");
        }
    }
}
