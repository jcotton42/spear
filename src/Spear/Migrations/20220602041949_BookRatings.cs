using Microsoft.EntityFrameworkCore.Migrations;
using Spear.Models;

#nullable disable

namespace Spear.Migrations
{
    public partial class BookRatings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:book_type", "book,fic,meme")
                .Annotation("Npgsql:Enum:permission", "moderate_prompts,submit_prompts,moderate_books")
                .Annotation("Npgsql:Enum:permission_mode", "allow,deny")
                .Annotation("Npgsql:Enum:rating", "general,teen,mature,explicit")
                .OldAnnotation("Npgsql:Enum:book_type", "book,fic,meme")
                .OldAnnotation("Npgsql:Enum:permission", "moderate_prompts,submit_prompts")
                .OldAnnotation("Npgsql:Enum:permission_mode", "allow,deny");

            migrationBuilder.AddColumn<Rating>(
                name: "rating",
                table: "books",
                type: "rating",
                nullable: false,
                defaultValue: Rating.General);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "rating",
                table: "books");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:book_type", "book,fic,meme")
                .Annotation("Npgsql:Enum:permission", "moderate_prompts,submit_prompts")
                .Annotation("Npgsql:Enum:permission_mode", "allow,deny")
                .OldAnnotation("Npgsql:Enum:book_type", "book,fic,meme")
                .OldAnnotation("Npgsql:Enum:permission", "moderate_prompts,submit_prompts,moderate_books")
                .OldAnnotation("Npgsql:Enum:permission_mode", "allow,deny")
                .OldAnnotation("Npgsql:Enum:rating", "general,teen,mature,explicit");
        }
    }
}
