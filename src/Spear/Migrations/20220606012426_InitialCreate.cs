using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Spear.Models;

#nullable disable

namespace Spear.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:book_type", "book,fic,meme")
                .Annotation("Npgsql:Enum:permission", "moderate_prompts,submit_prompts,moderate_books")
                .Annotation("Npgsql:Enum:permission_mode", "allow,deny")
                .Annotation("Npgsql:Enum:rating", "general,teen,mature,explicit");

            migrationBuilder.CreateTable(
                name: "guilds",
                columns: table => new
                {
                    id = table.Column<ulong>(type: "numeric(20,0)", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_guilds", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "books",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    title = table.Column<string>(type: "text", nullable: false),
                    type = table.Column<BookType>(type: "book_type", nullable: false),
                    guild_id = table.Column<ulong>(type: "numeric(20,0)", nullable: true),
                    rating = table.Column<Rating>(type: "rating", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_books", x => x.id);
                    table.ForeignKey(
                        name: "fk_books_guilds_guild_id",
                        column: x => x.guild_id,
                        principalTable: "guilds",
                        principalColumn: "id");
                });

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

            migrationBuilder.CreateTable(
                name: "prompts",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    text = table.Column<string>(type: "text", nullable: false),
                    submitter = table.Column<ulong>(type: "numeric(20,0)", nullable: true),
                    guild_id = table.Column<ulong>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_prompts", x => x.id);
                    table.ForeignKey(
                        name: "fk_prompts_guilds_guild_id",
                        column: x => x.guild_id,
                        principalTable: "guilds",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_books_guild_id_title_type",
                table: "books",
                columns: new[] { "guild_id", "title", "type" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_prompts_guild_id",
                table: "prompts",
                column: "guild_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "books");

            migrationBuilder.DropTable(
                name: "permission_defaults");

            migrationBuilder.DropTable(
                name: "permission_entries");

            migrationBuilder.DropTable(
                name: "prompts");

            migrationBuilder.DropTable(
                name: "guilds");

            migrationBuilder.AlterDatabase()
                .OldAnnotation("Npgsql:Enum:book_type", "book,fic,meme")
                .OldAnnotation("Npgsql:Enum:permission", "moderate_prompts,submit_prompts,moderate_books")
                .OldAnnotation("Npgsql:Enum:permission_mode", "allow,deny")
                .OldAnnotation("Npgsql:Enum:rating", "general,teen,mature,explicit");
        }
    }
}
