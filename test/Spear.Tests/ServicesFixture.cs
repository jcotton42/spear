using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Commands.Contexts;
using Respawn;
using Spear.Models;
using Spear.Services;

namespace Spear.Tests;

public class ServicesFixture : IAsyncLifetime {
    private readonly PostgreSqlTestcontainer _dbContainer =
        // see https://github.com/testcontainers/testcontainers-dotnet/issues/750 once 2.5.0 is out
#pragma warning disable 618
        new ContainerBuilder<PostgreSqlTestcontainer>()
#pragma warning restore 618
            .WithDatabase(new PostgreSqlTestcontainerConfiguration("postgres:14") {
                Database = "spear",
                Username = "postgres",
                Password = "postgres",
            }).Build();

    public Respawner DbRespawner { get; private set; } = default!;
    public ServiceProvider Services { get; private set; } = default!;

    public async Task InitializeAsync() {
        await _dbContainer.StartAsync();

        Services = new ServiceCollection()
            .AddLogging()
            .AddDbContext<SpearContext>(options => options.UseNpgsql(_dbContainer.ConnectionString))
            .AddScoped<GuildService>()
            .AddScopedMocked<IDiscordRestChannelAPI>()
            .AddScopedMocked<IDiscordRestGuildAPI>()
            .AddScopedMocked<ISpearOperationContext>()
            .AddScoped<AuthorizationService>()
            .BuildServiceProvider();

        using var scope = Services.CreateScope();
        using var context = scope.ServiceProvider.GetRequiredService<SpearContext>();

        await context.Database.EnsureCreatedAsync();

        DbRespawner = await Respawner.CreateAsync(context.Database.GetDbConnection(), new RespawnerOptions {
            SchemasToInclude = new[] { "public" },
            DbAdapter = DbAdapter.Postgres,
        });
    }

    public async Task DisposeAsync() {
        await _dbContainer.DisposeAsync();
    }
}
