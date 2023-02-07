using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using Microsoft.EntityFrameworkCore;
using Respawn;
using Spear.Models;

namespace Spear.Tests;

public class DbFixture : IAsyncLifetime {
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
    public SpearContext DbContext { get; private set; } = default!;

    public async Task InitializeAsync() {
        await _dbContainer.StartAsync();
        DbContext = new SpearContext(new DbContextOptionsBuilder<SpearContext>().UseNpgsql(_dbContainer.ConnectionString).Options);
        await DbContext.Database.EnsureCreatedAsync();

        DbRespawner = await Respawner.CreateAsync(DbContext.Database.GetDbConnection(), new RespawnerOptions {
            SchemasToInclude = new[] { "public" },
            DbAdapter = DbAdapter.Postgres,
        });
    }

    public async Task DisposeAsync() {
        await DbContext.DisposeAsync();
        await _dbContainer.DisposeAsync();
    }
}
