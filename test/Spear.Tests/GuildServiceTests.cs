using Bogus;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Remora.Discord.API;
using Spear.Models;
using Spear.Services;

namespace Spear.Tests;

public class GuildServiceTests : TestBase, IClassFixture<ServicesFixture> {
    private readonly SpearContext _dbContext;
    private readonly Faker<Guild> _guildFaker;
    private readonly GuildService _guilds;

    public GuildServiceTests(ServicesFixture servicesFixture) : base(servicesFixture) {
        _guildFaker = new Faker<Guild>()
            .RuleFor(g => g.Id, faker => DiscordSnowflake.New(faker.Random.ULong()))
            .RuleFor(g => g.Name, faker => faker.Company.CompanyName());

        _dbContext = _scope.ServiceProvider.GetRequiredService<SpearContext>();
        _guilds = _scope.ServiceProvider.GetRequiredService<GuildService>();
    }

    [Fact]
    public async Task RegisteredGuildCanBeFound() {
        var insertedGuild = _guildFaker.Generate();
        _dbContext.Guilds.Add(insertedGuild);
        await _dbContext.SaveChangesAsync();

        (await _guilds.IsGuildRegisteredAsync(insertedGuild.Id, CancellationToken.None)).Should().BeTrue();
    }

    [Fact]
    public async Task UpsertInsertsWhenNoMatch() {
        var newGuild = _guildFaker.Generate();

        await _guilds.UpsertGuildAsync(newGuild.Id, newGuild.Name, CancellationToken.None);
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

        await _guilds.UpsertGuildAsync(newGuild.Id, newGuild.Name, CancellationToken.None);
        _dbContext.ChangeTracker.Clear();
        var retreived = await _dbContext.Guilds.FirstOrDefaultAsync(g => g.Id == newGuild.Id);

        retreived.Should().NotBeNull();
        retreived!.Name.Should().BeEquivalentTo(newGuild.Name);
    }
}
