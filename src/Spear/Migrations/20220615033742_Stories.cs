using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Spear.Models;

#nullable disable

namespace Spear.Migrations
{
    public partial class Stories : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:book_type", "book,fic,meme")
                .Annotation("Npgsql:Enum:permission", "moderate_prompts,submit_prompts,moderate_books")
                .Annotation("Npgsql:Enum:permission_mode", "allow,deny")
                .Annotation("Npgsql:Enum:rating", "general,teen,mature,explicit")
                .Annotation("Npgsql:Enum:reaction", "like,dislike,indifferent")
                .Annotation("Npgsql:Enum:story_status", "complete,in_progress,hiatus,dead")
                .Annotation("Npgsql:Enum:tag_type", "general,fandom,ship")
                .Annotation("Npgsql:PostgresExtension:pg_trgm", ",,")
                .OldAnnotation("Npgsql:Enum:book_type", "book,fic,meme")
                .OldAnnotation("Npgsql:Enum:permission", "moderate_prompts,submit_prompts,moderate_books")
                .OldAnnotation("Npgsql:Enum:permission_mode", "allow,deny")
                .OldAnnotation("Npgsql:Enum:rating", "general,teen,mature,explicit");

            migrationBuilder.CreateTable(
                name: "authors",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    guild_id = table.Column<ulong>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_authors", x => x.id);
                    table.ForeignKey(
                        name: "fk_authors_guilds_guild_id",
                        column: x => x.guild_id,
                        principalTable: "guilds",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tags",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    type = table.Column<TagType>(type: "tag_type", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tags", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "author_profiles",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    author_id = table.Column<int>(type: "integer", nullable: false),
                    pseud = table.Column<string>(type: "text", nullable: false),
                    url = table.Column<string>(type: "text", nullable: false),
                    url_is_canonical = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_author_profiles", x => x.id);
                    table.ForeignKey(
                        name: "fk_author_profiles_authors_author_id",
                        column: x => x.author_id,
                        principalTable: "authors",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "stories",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    guild_id = table.Column<ulong>(type: "numeric(20,0)", nullable: false),
                    title = table.Column<string>(type: "text", nullable: false),
                    author_id = table.Column<int>(type: "integer", nullable: false),
                    summary = table.Column<string>(type: "text", nullable: true),
                    rating = table.Column<Rating>(type: "rating", nullable: false),
                    status = table.Column<StoryStatus>(type: "story_status", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_stories", x => x.id);
                    table.ForeignKey(
                        name: "fk_stories_authors_author_id",
                        column: x => x.author_id,
                        principalTable: "authors",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_stories_guilds_guild_id",
                        column: x => x.guild_id,
                        principalTable: "guilds",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "story_reactions",
                columns: table => new
                {
                    story_id = table.Column<int>(type: "integer", nullable: false),
                    user_id = table.Column<ulong>(type: "numeric(20,0)", nullable: false),
                    reaction = table.Column<Reaction>(type: "reaction", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_story_reactions", x => new { x.story_id, x.user_id });
                    table.ForeignKey(
                        name: "fk_story_reactions_stories_story_id",
                        column: x => x.story_id,
                        principalTable: "stories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "story_tag",
                columns: table => new
                {
                    stories_id = table.Column<int>(type: "integer", nullable: false),
                    tags_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_story_tag", x => new { x.stories_id, x.tags_id });
                    table.ForeignKey(
                        name: "fk_story_tag_stories_stories_id",
                        column: x => x.stories_id,
                        principalTable: "stories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_story_tag_tags_tags_id",
                        column: x => x.tags_id,
                        principalTable: "tags",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "story_urls",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    story_id = table.Column<int>(type: "integer", nullable: false),
                    url = table.Column<string>(type: "text", nullable: false),
                    is_canonical = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_story_urls", x => x.id);
                    table.ForeignKey(
                        name: "fk_story_urls_stories_story_id",
                        column: x => x.story_id,
                        principalTable: "stories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_author_profiles_author_id",
                table: "author_profiles",
                column: "author_id");

            migrationBuilder.CreateIndex(
                name: "ix_authors_guild_id",
                table: "authors",
                column: "guild_id");

            migrationBuilder.CreateIndex(
                name: "ix_stories_author_id",
                table: "stories",
                column: "author_id");

            migrationBuilder.CreateIndex(
                name: "ix_stories_guild_id",
                table: "stories",
                column: "guild_id");

            migrationBuilder.CreateIndex(
                name: "ix_stories_title",
                table: "stories",
                column: "title")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "ix_story_tag_tags_id",
                table: "story_tag",
                column: "tags_id");

            migrationBuilder.CreateIndex(
                name: "ix_story_urls_story_id",
                table: "story_urls",
                column: "story_id");

            migrationBuilder.CreateIndex(
                name: "ix_tags_name",
                table: "tags",
                column: "name")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "ix_tags_name_type",
                table: "tags",
                columns: new[] { "name", "type" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "author_profiles");

            migrationBuilder.DropTable(
                name: "story_reactions");

            migrationBuilder.DropTable(
                name: "story_tag");

            migrationBuilder.DropTable(
                name: "story_urls");

            migrationBuilder.DropTable(
                name: "tags");

            migrationBuilder.DropTable(
                name: "stories");

            migrationBuilder.DropTable(
                name: "authors");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:book_type", "book,fic,meme")
                .Annotation("Npgsql:Enum:permission", "moderate_prompts,submit_prompts,moderate_books")
                .Annotation("Npgsql:Enum:permission_mode", "allow,deny")
                .Annotation("Npgsql:Enum:rating", "general,teen,mature,explicit")
                .OldAnnotation("Npgsql:Enum:book_type", "book,fic,meme")
                .OldAnnotation("Npgsql:Enum:permission", "moderate_prompts,submit_prompts,moderate_books")
                .OldAnnotation("Npgsql:Enum:permission_mode", "allow,deny")
                .OldAnnotation("Npgsql:Enum:rating", "general,teen,mature,explicit")
                .OldAnnotation("Npgsql:Enum:reaction", "like,dislike,indifferent")
                .OldAnnotation("Npgsql:Enum:story_status", "complete,in_progress,hiatus,dead")
                .OldAnnotation("Npgsql:Enum:tag_type", "general,fandom,ship")
                .OldAnnotation("Npgsql:PostgresExtension:pg_trgm", ",,");
        }
    }
}
