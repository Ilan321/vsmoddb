using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VsModDb.Data.Migrations
{
    /// <inheritdoc />
    public partial class AccountLinkRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AccountLinkRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TimeCreatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    Username = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    LinkToken = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Secret = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountLinkRequests", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccountLinkRequests_LinkToken",
                table: "AccountLinkRequests",
                column: "LinkToken",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccountLinkRequests");
        }
    }
}
