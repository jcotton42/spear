using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Spear.Migrations
{
    public partial class StoryPermissions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:book_type", "book,fic,meme")
                .Annotation("Npgsql:Enum:permission", "moderate_prompts,submit_prompts,moderate_books,submit_stories,moderate_stories")
                .Annotation("Npgsql:Enum:permission_mode", "allow,deny")
                .Annotation("Npgsql:Enum:rating", "general,teen,mature,explicit")
                .Annotation("Npgsql:Enum:reaction", "like,dislike,indifferent")
                .Annotation("Npgsql:Enum:story_status", "complete,in_progress,hiatus,dead")
                .Annotation("Npgsql:Enum:tag_type", "general,fandom,ship")
                .Annotation("Npgsql:PostgresExtension:pg_trgm", ",,")
                .OldAnnotation("Npgsql:Enum:book_type", "book,fic,meme")
                .OldAnnotation("Npgsql:Enum:permission", "moderate_prompts,submit_prompts,moderate_books")
                .OldAnnotation("Npgsql:Enum:permission_mode", "allow,deny")
                .OldAnnotation("Npgsql:Enum:rating", "general,teen,mature,explicit")
                .OldAnnotation("Npgsql:Enum:reaction", "like,dislike,indifferent")
                .OldAnnotation("Npgsql:Enum:story_status", "complete,in_progress,hiatus,dead")
                .OldAnnotation("Npgsql:Enum:tag_type", "general,fandom,ship")
                .OldAnnotation("Npgsql:PostgresExtension:pg_trgm", ",,");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
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
                .OldAnnotation("Npgsql:Enum:permission", "moderate_prompts,submit_prompts,moderate_books,submit_stories,moderate_stories")
                .OldAnnotation("Npgsql:Enum:permission_mode", "allow,deny")
                .OldAnnotation("Npgsql:Enum:rating", "general,teen,mature,explicit")
                .OldAnnotation("Npgsql:Enum:reaction", "like,dislike,indifferent")
                .OldAnnotation("Npgsql:Enum:story_status", "complete,in_progress,hiatus,dead")
                .OldAnnotation("Npgsql:Enum:tag_type", "general,fandom,ship")
                .OldAnnotation("Npgsql:PostgresExtension:pg_trgm", ",,");
        }
    }
}
