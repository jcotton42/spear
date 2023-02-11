using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Respawn;
using Spear.Models;

namespace Spear.Tests;

public abstract class TestBase : IAsyncLifetime {
    protected Respawner _dbRespawner;
    protected IServiceScope _scope;

    public TestBase(ServicesFixture servicesFixture) {
        _dbRespawner = servicesFixture.DbRespawner;
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
