using Bogus;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Remora.Discord.API;
using Spear.Models;
using Spear.Services;

namespace Spear.Tests;

public class GuildServiceTests : TestBase, IClassFixture<DbFixture> {
    private readonly GuildService _guild;
    private readonly Faker<Guild> _guildFaker;

    public GuildServiceTests(DbFixture dbFixture) : base(dbFixture) {
        _guild = new GuildService(NullLogger<GuildService>.Instance, dbFixture.DbContext);

        _guildFaker = new Faker<Guild>()
            .RuleFor(g => g.Id, faker => DiscordSnowflake.New(faker.Random.ULong()))
            .RuleFor(g => g.Name, faker => faker.Company.CompanyName());
    }

    [Fact]
    public async Task RegisteredGuildCanBeFound() {
        var insertedGuild = _guildFaker.Generate();
        _dbContext.Guilds.Add(insertedGuild);
        await _dbContext.SaveChangesAsync();

        (await _guild.IsGuildRegisteredAsync(insertedGuild.Id, CancellationToken.None)).Should().BeTrue();
    }

    [Fact]
    public async Task UpsertInsertsWhenNoMatch() {
        var newGuild = _guildFaker.Generate();
        await _guild.UpsertGuildAsync(newGuild.Id, newGuild.Name, CancellationToken.None);

        _dbContext.ChangeTracker.Clear();

        var guild = await _dbContext.Guilds.FirstOrDefaultAsync(g => g.Id == newGuild.Id);
        guild.Should().NotBeNull();
        guild!.Id.Should().BeEquivalentTo(newGuild.Id);
        guild.Name.Should().BeEquivalentTo(newGuild.Name);
    }

    [Fact]
    public async Task UpsertUpdatesWhenIdMatches() {
        var initialGuild = _guildFaker.Generate();
        _dbContext.Guilds.Add(initialGuild);
        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();
        var newGuild = _guildFaker.Generate();
        newGuild.Id = initialGuild.Id;

        await _guild.UpsertGuildAsync(newGuild.Id, newGuild.Name, CancellationToken.None);
        _dbContext.ChangeTracker.Clear();
        var retreived = await _dbContext.Guilds.FirstOrDefaultAsync(g => g.Id == newGuild.Id);

        retreived.Should().NotBeNull();
        retreived!.Name.Should().BeEquivalentTo(newGuild.Name);
    }
}
