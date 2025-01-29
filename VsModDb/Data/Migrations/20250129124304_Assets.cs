using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VsModDb.Data.Migrations
{
    /// <inheritdoc />
    public partial class Assets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BannerId",
                table: "Mods",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Asset",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FileName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    AssetPath = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Asset", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Mods_BannerId",
                table: "Mods",
                column: "BannerId",
                unique: true,
                filter: "[BannerId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Mods_Asset_BannerId",
                table: "Mods",
                column: "BannerId",
                principalTable: "Asset",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Mods_Asset_BannerId",
                table: "Mods");

            migrationBuilder.DropTable(
                name: "Asset");

            migrationBuilder.DropIndex(
                name: "IX_Mods_BannerId",
                table: "Mods");

            migrationBuilder.DropColumn(
                name: "BannerId",
                table: "Mods");
        }
    }
}
