using EntityFramework.Exceptions.PostgreSQL;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Remora.Rest.Core;

namespace Spear.Models;

public class SpearContext : DbContext {
    public DbSet<Author> Authors { get; set; } = null!;
    public DbSet<AuthorProfile> AuthorProfiles { get; set; } = null!;
    public DbSet<Book> Books { get; set; } = null!;
    public DbSet<Guild> Guilds { get; set; } = null!;
    public DbSet<PermissionDefault> PermissionDefaults { get; set; } = null!;
    public DbSet<PermissionEntry> PermissionEntries { get; set; } = null!;
    public DbSet<Prompt> Prompts { get; set; } = null!;
    public DbSet<Story> Stories { get; set; } = null!;
    public DbSet<StoryReaction> StoryReactions { get; set; } = null!;
    public DbSet<StoryUrl> StoryUrls { get; set; } = null!;
    public DbSet<Tag> Tags { get; set; } = null!;

    static SpearContext() {
        NpgsqlConnection.GlobalTypeMapper
            .MapEnum<BookType>()
            .MapEnum<Permission>()
            .MapEnum<PermissionMode>()
            .MapEnum<Rating>()
            .MapEnum<Reaction>()
            .MapEnum<StoryStatus>()
            .MapEnum<TagType>();
    }

    public SpearContext(DbContextOptions<SpearContext> options) : base(options) {}

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder) {
        configurationBuilder
            .Properties<Snowflake>()
            .HaveConversion<DiscordSnowflakeConverter>();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options) {
        options
            .UseExceptionProcessor()
            .UseSnakeCaseNamingConvention();
    }

    protected override void OnModelCreating(ModelBuilder builder) {
        builder
            .HasPostgresExtension("pg_trgm");

        builder
            .HasPostgresEnum<BookType>()
            .HasPostgresEnum<Permission>()
            .HasPostgresEnum<PermissionMode>()
            .HasPostgresEnum<Rating>()
            .HasPostgresEnum<Reaction>()
            .HasPostgresEnum<StoryStatus>()
            .HasPostgresEnum<TagType>();

        builder.Entity<Book>()
            .HasIndex(b => new {b.GuildId, b.Title, b.Type})
            .IsUnique();

        builder.Entity<Guild>()
            .Property(g => g.SafeChannelRatingCap)
            .HasDefaultValue(Rating.Teen);
        builder.Entity<Guild>()
            .Property(g => g.NsfwChannelRatingCap)
            .HasDefaultValue(Rating.Mature);

        builder.Entity<PermissionDefault>()
            .HasKey(pd => new {pd.GuildId, pd.Permission});

        builder.Entity<PermissionEntry>()
            .HasKey(pe => new {pe.GuildId, pe.RoleId, pe.Permission});

        builder.Entity<Story>()
            .HasIndex(s => s.Title)
            .HasMethod("gin")
            .HasOperators("gin_trgm_ops");

        builder.Entity<StoryReaction>()
            .HasKey(sr => new {sr.StoryId, sr.UserId});

        builder.Entity<Tag>()
            .HasIndex(t => new {t.Name, t.Type})
            .IsUnique();
        builder.Entity<Tag>()
            .HasIndex(t => t.Name)
            .HasMethod("gin")
            .HasOperators("gin_trgm_ops");
    }
}
