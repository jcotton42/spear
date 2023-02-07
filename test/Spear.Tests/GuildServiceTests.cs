using Bogus;
using FluentAssertions;
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
}
