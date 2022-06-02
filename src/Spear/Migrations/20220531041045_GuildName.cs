using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Spear.Migrations
{
    public partial class GuildName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "name",
                table: "guilds",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "name",
                table: "guilds");
        }
    }
}
