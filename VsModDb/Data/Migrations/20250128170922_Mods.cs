using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VsModDb.Data.Migrations
{
    /// <inheritdoc />
    public partial class Mods : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Mods",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Summary = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    UrlAlias = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    TimeCreatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    TimeUpdatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    Description = table.Column<string>(type: "nvarchar(max)", maxLength: -1, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Mods", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ModTag",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ModId = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModTag", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ModTag_Mods_ModId",
                        column: x => x.ModId,
                        principalTable: "Mods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Mods_Name",
                table: "Mods",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Mods_TimeCreatedUtc",
                table: "Mods",
                column: "TimeCreatedUtc");

            migrationBuilder.CreateIndex(
                name: "IX_Mods_TimeUpdatedUtc",
                table: "Mods",
                column: "TimeUpdatedUtc");

            migrationBuilder.CreateIndex(
                name: "IX_Mods_UrlAlias",
                table: "Mods",
                column: "UrlAlias");

            migrationBuilder.CreateIndex(
                name: "IX_ModTag_ModId",
                table: "ModTag",
                column: "ModId");

            migrationBuilder.CreateIndex(
                name: "IX_ModTag_Value",
                table: "ModTag",
                column: "Value");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ModTag");

            migrationBuilder.DropTable(
                name: "Mods");
        }
    }
}
