using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Spear.Migrations
{
    public partial class StoryUrlColumnRename : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "is_canonical",
                table: "story_urls",
                newName: "is_normalized");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "is_normalized",
                table: "story_urls",
                newName: "is_canonical");
        }
    }
}
