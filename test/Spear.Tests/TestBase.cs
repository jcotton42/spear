using Bogus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Remora.Discord.API;
using Respawn;
using Spear.Models;

namespace Spear.Tests;

public abstract class TestBase : IAsyncLifetime {
    protected readonly Respawner _dbRespawner;
    protected readonly Faker<Guild> _guildFaker;
    protected readonly IServiceScope _scope;

    public TestBase(ServicesFixture servicesFixture) {
        _dbRespawner = servicesFixture.DbRespawner;
        _guildFaker = new Faker<Guild>()
            .RuleFor(g => g.Id, faker => DiscordSnowflake.New(faker.Random.ULong()))
            .RuleFor(g => g.Name, faker => faker.Company.CompanyName());
        _scope = servicesFixture.Services.CreateScope();
    }

    public async Task InitializeAsync() {
        var context = _scope.ServiceProvider.GetRequiredService<SpearContext>();
        await context.Database.OpenConnectionAsync();
        await _dbRespawner.ResetAsync(context.Database.GetDbConnection());
    }

    public Task DisposeAsync() {
        _scope.Dispose();
        return Task.CompletedTask;
    }
}
