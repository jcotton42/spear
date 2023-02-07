using Microsoft.EntityFrameworkCore;
using Respawn;
using Spear.Models;

namespace Spear.Tests;

public abstract class TestBase : IAsyncLifetime {
    protected SpearContext _dbContext;
    protected Respawner _dbRespawner;

    public TestBase(DbFixture dbFixture) {
        _dbContext = dbFixture.DbContext;
        _dbRespawner = dbFixture.DbRespawner;
    }

    public async Task InitializeAsync() {
        await _dbRespawner.ResetAsync(_dbContext.Database.GetDbConnection());
    }

    public Task DisposeAsync() => Task.CompletedTask;
}
