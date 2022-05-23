using EntityFramework.Exceptions.PostgreSQL;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Remora.Rest.Core;

namespace Spear.Models;

public class SpearContext : DbContext {
    public DbSet<Book> Books { get; set; } = null!;
    public DbSet<Guild> Guilds { get; set; } = null!;
    public DbSet<Prompt> Prompts { get; set; } = null!;

    static SpearContext() {
        NpgsqlConnection.GlobalTypeMapper.MapEnum<BookType>();
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
        builder.HasPostgresEnum<BookType>();

        builder.Entity<Book>()
            .HasIndex(b => new { b.GuildId, b.Title, b.Type })
            .IsUnique();
    }
}
