using Microsoft.EntityFrameworkCore.Migrations;
using Spear.Models;

#nullable disable

namespace Spear.Migrations
{
    public partial class GuildSettings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Rating>(
                name: "nsfw_channel_rating_cap",
                table: "guilds",
                type: "rating",
                nullable: false,
                defaultValue: Rating.Mature);

            migrationBuilder.AddColumn<Rating>(
                name: "safe_channel_rating_cap",
                table: "guilds",
                type: "rating",
                nullable: false,
                defaultValue: Rating.Teen);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "nsfw_channel_rating_cap",
                table: "guilds");

            migrationBuilder.DropColumn(
                name: "safe_channel_rating_cap",
                table: "guilds");
        }
    }
}
