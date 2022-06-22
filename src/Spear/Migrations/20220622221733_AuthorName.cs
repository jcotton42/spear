using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Spear.Migrations
{
    public partial class AuthorName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "name",
                table: "authors",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "name",
                table: "authors");
        }
    }
}
