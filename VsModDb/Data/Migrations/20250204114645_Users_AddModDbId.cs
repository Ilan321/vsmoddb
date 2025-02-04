using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VsModDb.Data.Migrations
{
    /// <inheritdoc />
    public partial class Users_AddModDbId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ModDbUserId",
                table: "AspNetUsers",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ModDbUserId",
                table: "AspNetUsers");
        }
    }
}
