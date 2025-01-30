using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VsModDb.Data.Migrations
{
    /// <inheritdoc />
    public partial class ModComments_ContentType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContentType",
                table: "ModComments",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "Html");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContentType",
                table: "ModComments");
        }
    }
}
