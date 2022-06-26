﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Spear.Models;

#nullable disable

namespace Spear.Migrations
{
    [DbContext(typeof(SpearContext))]
    partial class SpearContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.5")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.HasPostgresEnum(modelBuilder, "book_type", new[] { "book", "fic", "meme" });
            NpgsqlModelBuilderExtensions.HasPostgresEnum(modelBuilder, "permission", new[] { "moderate_prompts", "submit_prompts", "moderate_books", "submit_stories", "moderate_stories" });
            NpgsqlModelBuilderExtensions.HasPostgresEnum(modelBuilder, "permission_mode", new[] { "allow", "deny" });
            NpgsqlModelBuilderExtensions.HasPostgresEnum(modelBuilder, "rating", new[] { "general", "teen", "mature", "explicit" });
            NpgsqlModelBuilderExtensions.HasPostgresEnum(modelBuilder, "reaction", new[] { "like", "dislike", "indifferent" });
            NpgsqlModelBuilderExtensions.HasPostgresEnum(modelBuilder, "story_status", new[] { "complete", "in_progress", "hiatus", "dead" });
            NpgsqlModelBuilderExtensions.HasPostgresEnum(modelBuilder, "tag_type", new[] { "general", "fandom", "ship" });
            NpgsqlModelBuilderExtensions.HasPostgresExtension(modelBuilder, "pg_trgm");
            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Spear.Models.Author", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<ulong>("GuildId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("guild_id");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("name");

                    b.HasKey("Id")
                        .HasName("pk_authors");

                    b.HasIndex("GuildId")
                        .HasDatabaseName("ix_authors_guild_id");

                    b.ToTable("authors", (string)null);
                });

            modelBuilder.Entity("Spear.Models.AuthorProfile", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("AuthorId")
                        .HasColumnType("integer")
                        .HasColumnName("author_id");

                    b.Property<string>("Pseud")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("pseud");

                    b.Property<string>("Url")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("url");

                    b.Property<bool>("UrlIsCanonical")
                        .HasColumnType("boolean")
                        .HasColumnName("url_is_canonical");

                    b.HasKey("Id")
                        .HasName("pk_author_profiles");

                    b.HasIndex("AuthorId")
                        .HasDatabaseName("ix_author_profiles_author_id");

                    b.ToTable("author_profiles", (string)null);
                });

            modelBuilder.Entity("Spear.Models.Book", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<ulong?>("GuildId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("guild_id");

                    b.Property<Rating>("Rating")
                        .HasColumnType("rating")
                        .HasColumnName("rating");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("title");

                    b.Property<BookType>("Type")
                        .HasColumnType("book_type")
                        .HasColumnName("type");

                    b.HasKey("Id")
                        .HasName("pk_books");

                    b.HasIndex("GuildId", "Title", "Type")
                        .IsUnique()
                        .HasDatabaseName("ix_books_guild_id_title_type");

                    b.ToTable("books", (string)null);
                });

            modelBuilder.Entity("Spear.Models.Guild", b =>
                {
                    b.Property<ulong>("Id")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("id");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("name");

                    b.Property<Rating>("NsfwChannelRatingCap")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("rating")
                        .HasDefaultValue(Rating.Mature)
                        .HasColumnName("nsfw_channel_rating_cap");

                    b.Property<Rating>("SafeChannelRatingCap")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("rating")
                        .HasDefaultValue(Rating.Teen)
                        .HasColumnName("safe_channel_rating_cap");

                    b.HasKey("Id")
                        .HasName("pk_guilds");

                    b.ToTable("guilds", (string)null);
                });

            modelBuilder.Entity("Spear.Models.PermissionDefault", b =>
                {
                    b.Property<ulong>("GuildId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("guild_id");

                    b.Property<Permission>("Permission")
                        .HasColumnType("permission")
                        .HasColumnName("permission");

                    b.Property<PermissionMode>("Mode")
                        .HasColumnType("permission_mode")
                        .HasColumnName("mode");

                    b.HasKey("GuildId", "Permission")
                        .HasName("pk_permission_defaults");

                    b.ToTable("permission_defaults", (string)null);
                });

            modelBuilder.Entity("Spear.Models.PermissionEntry", b =>
                {
                    b.Property<ulong>("GuildId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("guild_id");

                    b.Property<ulong>("RoleId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("role_id");

                    b.Property<Permission>("Permission")
                        .HasColumnType("permission")
                        .HasColumnName("permission");

                    b.Property<PermissionMode>("Mode")
                        .HasColumnType("permission_mode")
                        .HasColumnName("mode");

                    b.HasKey("GuildId", "RoleId", "Permission")
                        .HasName("pk_permission_entries");

                    b.ToTable("permission_entries", (string)null);
                });

            modelBuilder.Entity("Spear.Models.Prompt", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<ulong>("GuildId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("guild_id");

                    b.Property<ulong?>("Submitter")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("submitter");

                    b.Property<string>("Text")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("text");

                    b.HasKey("Id")
                        .HasName("pk_prompts");

                    b.HasIndex("GuildId")
                        .HasDatabaseName("ix_prompts_guild_id");

                    b.ToTable("prompts", (string)null);
                });

            modelBuilder.Entity("Spear.Models.Story", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("AuthorId")
                        .HasColumnType("integer")
                        .HasColumnName("author_id");

                    b.Property<ulong>("GuildId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("guild_id");

                    b.Property<Rating>("Rating")
                        .HasColumnType("rating")
                        .HasColumnName("rating");

                    b.Property<StoryStatus>("Status")
                        .HasColumnType("story_status")
                        .HasColumnName("status");

                    b.Property<string>("Summary")
                        .HasColumnType("text")
                        .HasColumnName("summary");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("title");

                    b.HasKey("Id")
                        .HasName("pk_stories");

                    b.HasIndex("AuthorId")
                        .HasDatabaseName("ix_stories_author_id");

                    b.HasIndex("GuildId")
                        .HasDatabaseName("ix_stories_guild_id");

                    b.HasIndex("Title")
                        .HasDatabaseName("ix_stories_title");

                    NpgsqlIndexBuilderExtensions.HasMethod(b.HasIndex("Title"), "gin");
                    NpgsqlIndexBuilderExtensions.HasOperators(b.HasIndex("Title"), new[] { "gin_trgm_ops" });

                    b.ToTable("stories", (string)null);
                });

            modelBuilder.Entity("Spear.Models.StoryReaction", b =>
                {
                    b.Property<int>("StoryId")
                        .HasColumnType("integer")
                        .HasColumnName("story_id");

                    b.Property<ulong>("UserId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("user_id");

                    b.Property<Reaction>("Reaction")
                        .HasColumnType("reaction")
                        .HasColumnName("reaction");

                    b.HasKey("StoryId", "UserId")
                        .HasName("pk_story_reactions");

                    b.ToTable("story_reactions", (string)null);
                });

            modelBuilder.Entity("Spear.Models.StoryUrl", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<bool>("IsNormalized")
                        .HasColumnType("boolean")
                        .HasColumnName("is_normalized");

                    b.Property<int>("StoryId")
                        .HasColumnType("integer")
                        .HasColumnName("story_id");

                    b.Property<string>("Url")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("url");

                    b.HasKey("Id")
                        .HasName("pk_story_urls");

                    b.HasIndex("StoryId")
                        .HasDatabaseName("ix_story_urls_story_id");

                    b.ToTable("story_urls", (string)null);
                });

            modelBuilder.Entity("Spear.Models.Tag", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("name");

                    b.Property<TagType>("Type")
                        .HasColumnType("tag_type")
                        .HasColumnName("type");

                    b.HasKey("Id")
                        .HasName("pk_tags");

                    b.HasIndex("Name")
                        .HasDatabaseName("ix_tags_name");

                    NpgsqlIndexBuilderExtensions.HasMethod(b.HasIndex("Name"), "gin");
                    NpgsqlIndexBuilderExtensions.HasOperators(b.HasIndex("Name"), new[] { "gin_trgm_ops" });

                    b.HasIndex("Name", "Type")
                        .IsUnique()
                        .HasDatabaseName("ix_tags_name_type");

                    b.ToTable("tags", (string)null);
                });

            modelBuilder.Entity("StoryTag", b =>
                {
                    b.Property<int>("StoriesId")
                        .HasColumnType("integer")
                        .HasColumnName("stories_id");

                    b.Property<int>("TagsId")
                        .HasColumnType("integer")
                        .HasColumnName("tags_id");

                    b.HasKey("StoriesId", "TagsId")
                        .HasName("pk_story_tag");

                    b.HasIndex("TagsId")
                        .HasDatabaseName("ix_story_tag_tags_id");

                    b.ToTable("story_tag", (string)null);
                });

            modelBuilder.Entity("Spear.Models.Author", b =>
                {
                    b.HasOne("Spear.Models.Guild", null)
                        .WithMany("Authors")
                        .HasForeignKey("GuildId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_authors_guilds_guild_id");
                });

            modelBuilder.Entity("Spear.Models.AuthorProfile", b =>
                {
                    b.HasOne("Spear.Models.Author", null)
                        .WithMany("Profiles")
                        .HasForeignKey("AuthorId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_author_profiles_authors_author_id");
                });

            modelBuilder.Entity("Spear.Models.Book", b =>
                {
                    b.HasOne("Spear.Models.Guild", null)
                        .WithMany("Books")
                        .HasForeignKey("GuildId")
                        .HasConstraintName("fk_books_guilds_guild_id");
                });

            modelBuilder.Entity("Spear.Models.PermissionDefault", b =>
                {
                    b.HasOne("Spear.Models.Guild", null)
                        .WithMany("PermissionDefaults")
                        .HasForeignKey("GuildId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_permission_defaults_guilds_guild_id");
                });

            modelBuilder.Entity("Spear.Models.PermissionEntry", b =>
                {
                    b.HasOne("Spear.Models.Guild", null)
                        .WithMany("PermissionEntries")
                        .HasForeignKey("GuildId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_permission_entries_guilds_guild_id");
                });

            modelBuilder.Entity("Spear.Models.Prompt", b =>
                {
                    b.HasOne("Spear.Models.Guild", null)
                        .WithMany("Prompts")
                        .HasForeignKey("GuildId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_prompts_guilds_guild_id");
                });

            modelBuilder.Entity("Spear.Models.Story", b =>
                {
                    b.HasOne("Spear.Models.Author", "Author")
                        .WithMany("Stories")
                        .HasForeignKey("AuthorId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_stories_authors_author_id");

                    b.HasOne("Spear.Models.Guild", null)
                        .WithMany("Stories")
                        .HasForeignKey("GuildId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_stories_guilds_guild_id");

                    b.Navigation("Author");
                });

            modelBuilder.Entity("Spear.Models.StoryReaction", b =>
                {
                    b.HasOne("Spear.Models.Story", null)
                        .WithMany("Reactions")
                        .HasForeignKey("StoryId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_story_reactions_stories_story_id");
                });

            modelBuilder.Entity("Spear.Models.StoryUrl", b =>
                {
                    b.HasOne("Spear.Models.Story", null)
                        .WithMany("Urls")
                        .HasForeignKey("StoryId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_story_urls_stories_story_id");
                });

            modelBuilder.Entity("StoryTag", b =>
                {
                    b.HasOne("Spear.Models.Story", null)
                        .WithMany()
                        .HasForeignKey("StoriesId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_story_tag_stories_stories_id");

                    b.HasOne("Spear.Models.Tag", null)
                        .WithMany()
                        .HasForeignKey("TagsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_story_tag_tags_tags_id");
                });

            modelBuilder.Entity("Spear.Models.Author", b =>
                {
                    b.Navigation("Profiles");

                    b.Navigation("Stories");
                });

            modelBuilder.Entity("Spear.Models.Guild", b =>
                {
                    b.Navigation("Authors");

                    b.Navigation("Books");

                    b.Navigation("PermissionDefaults");

                    b.Navigation("PermissionEntries");

                    b.Navigation("Prompts");

                    b.Navigation("Stories");
                });

            modelBuilder.Entity("Spear.Models.Story", b =>
                {
                    b.Navigation("Reactions");

                    b.Navigation("Urls");
                });
#pragma warning restore 612, 618
        }
    }
}
