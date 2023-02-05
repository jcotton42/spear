using EntityFramework.Exceptions.PostgreSQL;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Remora.Rest.Core;

namespace Spear.Models;

public class SpearContext : DbContext {
    public DbSet<AuditEntry> AuditEntries => Set<AuditEntry>();
    public DbSet<Book> Books => Set<Book>();
    public DbSet<Guild> Guilds => Set<Guild>();
    public DbSet<PermissionDefault> PermissionDefaults => Set<PermissionDefault>();
    public DbSet<PermissionEntry> PermissionEntries => Set<PermissionEntry>();
    public DbSet<Prompt> Prompts => Set<Prompt>();

    static SpearContext() {
        NpgsqlConnection.GlobalTypeMapper
            .MapEnum<AuditEntryType>()
            .MapEnum<BookType>()
            .MapEnum<Permission>()
            .MapEnum<PermissionMode>()
            .MapEnum<Rating>();
    }

    public SpearContext(DbContextOptions<SpearContext> options) : base(options) { }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder) {
        configurationBuilder
            .Properties<Snowflake>()
            .HaveConversion<DiscordSnowflakeConverter>();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options) {
        options
            .UseNpgsql(o => o.UseNodaTime())
            .UseExceptionProcessor()
            .UseSnakeCaseNamingConvention();
    }

    protected override void OnModelCreating(ModelBuilder builder) {
        builder
            .HasPostgresEnum<AuditEntryType>()
            .HasPostgresEnum<BookType>()
            .HasPostgresEnum<Permission>()
            .HasPostgresEnum<PermissionMode>()
            .HasPostgresEnum<Rating>();

        builder.Entity<Book>()
            .HasIndex(b => new { b.GuildId, b.Title, b.Type })
            .IsUnique();

        builder.Entity<PermissionDefault>()
            .HasKey(pd => new { pd.GuildId, pd.Permission });

        builder.Entity<PermissionEntry>()
            .HasKey(pe => new { pe.GuildId, pe.RoleId, pe.Permission });
    }
}
