using Microsoft.EntityFrameworkCore;
using Remora.Rest.Core;

namespace Spear.Models;

public class SpearContext : DbContext {
    public DbSet<Guild> Guilds { get; set; } = null!;
    public DbSet<Prompt> Prompts { get; set; } = null!;

    public SpearContext(DbContextOptions<SpearContext> options) : base(options) {}

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder) {
        configurationBuilder
            .Properties<Snowflake>()
            .HaveConversion<DiscordSnowflakeConverter>();
    }
}
