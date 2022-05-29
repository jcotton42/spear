using Microsoft.EntityFrameworkCore.Migrations;
using Spear.Models;

#nullable disable

namespace Spear.Migrations
{
    public partial class Authorization : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:book_type", "book,fic,meme")
                .Annotation("Npgsql:Enum:permission", "moderate_prompts,submit_prompts")
                .Annotation("Npgsql:Enum:permission_mode", "allow,deny")
                .OldAnnotation("Npgsql:Enum:book_type", "book,fic,meme");

            migrationBuilder.CreateTable(
                name: "permission_defaults",
                columns: table => new
                {
                    guild_id = table.Column<ulong>(type: "numeric(20,0)", nullable: false),
                    permission = table.Column<Permission>(type: "permission", nullable: false),
                    mode = table.Column<PermissionMode>(type: "permission_mode", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_permission_defaults", x => new { x.guild_id, x.permission });
                    table.ForeignKey(
                        name: "fk_permission_defaults_guilds_guild_id",
                        column: x => x.guild_id,
                        principalTable: "guilds",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "permission_entries",
                columns: table => new
                {
                    guild_id = table.Column<ulong>(type: "numeric(20,0)", nullable: false),
                    role_id = table.Column<ulong>(type: "numeric(20,0)", nullable: false),
                    permission = table.Column<Permission>(type: "permission", nullable: false),
                    mode = table.Column<PermissionMode>(type: "permission_mode", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_permission_entries", x => new { x.guild_id, x.role_id, x.permission });
                    table.ForeignKey(
                        name: "fk_permission_entries_guilds_guild_id",
                        column: x => x.guild_id,
                        principalTable: "guilds",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "permission_defaults");

            migrationBuilder.DropTable(
                name: "permission_entries");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:book_type", "book,fic,meme")
                .OldAnnotation("Npgsql:Enum:book_type", "book,fic,meme")
                .OldAnnotation("Npgsql:Enum:permission", "moderate_prompts,submit_prompts")
                .OldAnnotation("Npgsql:Enum:permission_mode", "allow,deny");
        }
    }
}
