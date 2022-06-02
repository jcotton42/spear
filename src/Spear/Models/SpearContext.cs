using EntityFramework.Exceptions.PostgreSQL;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Remora.Rest.Core;

namespace Spear.Models;

public class SpearContext : DbContext {
    public DbSet<Book> Books { get; set; } = null!;
    public DbSet<Guild> Guilds { get; set; } = null!;
    public DbSet<PermissionDefault> PermissionDefaults { get; set; } = null!;
    public DbSet<PermissionEntry> PermissionEntries { get; set; } = null!;
    public DbSet<Prompt> Prompts { get; set; } = null!;

    static SpearContext() {
        NpgsqlConnection.GlobalTypeMapper
            .MapEnum<BookType>()
            .MapEnum<Permission>()
            .MapEnum<PermissionMode>()
            .MapEnum<Rating>();
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
            .HasPostgresEnum<BookType>()
            .HasPostgresEnum<Permission>()
            .HasPostgresEnum<PermissionMode>()
            .HasPostgresEnum<Rating>();

        builder.Entity<Book>()
            .HasIndex(b => new { b.GuildId, b.Title, b.Type })
            .IsUnique();

        builder.Entity<PermissionDefault>()
            .HasKey(pd => new {pd.GuildId, pd.Permission});

        builder.Entity<PermissionEntry>()
            .HasKey(pe => new {pe.GuildId, pe.RoleId, pe.Permission});
    }
}
