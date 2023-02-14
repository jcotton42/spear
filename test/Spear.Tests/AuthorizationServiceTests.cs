using AutoBogus;
using Bogus;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Remora.Discord.API;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Rest.Core;
using Remora.Results;
using Spear.Models;
using Spear.Services;

using RemoraGuild = Remora.Discord.API.Objects.Guild;

namespace Spear.Tests;

public class AuthorizationServiceTests : TestBase, IClassFixture<ServicesFixture> {
    private readonly AuthorizationService _authorization;
    private readonly Mock<IDiscordRestChannelAPI> _channelApiMock;
    private readonly SpearContext _dbContext;
    private readonly Mock<IDiscordRestGuildAPI> _guildApiMock;
    private readonly Mock<ISpearOperationContext> _operationContextMock;

    public AuthorizationServiceTests(ServicesFixture servicesFixture) : base(servicesFixture) {
        _authorization = _scope.ServiceProvider.GetRequiredService<AuthorizationService>();
        _channelApiMock = _scope.ServiceProvider.GetRequiredService<Mock<IDiscordRestChannelAPI>>();
        _dbContext = _scope.ServiceProvider.GetRequiredService<SpearContext>();
        _guildApiMock = _scope.ServiceProvider.GetRequiredService<Mock<IDiscordRestGuildAPI>>();
        _operationContextMock = _scope.ServiceProvider.GetRequiredService<Mock<ISpearOperationContext>>();
    }

    [Fact]
    public async Task HasSpearPermissionCheck() {
        var faker = new Faker();
        var userId = DiscordSnowflake.New(faker.Random.ULong());
        var spearGuild = _guildFaker.Generate();

        var discordGuildRoles = new AutoFaker<Role>()
            .RuleFor(r => r.ID, f => DiscordSnowflake.New(f.Random.ULong()))
            .RuleFor(r => r.Permissions, _ => DiscordPermissionSet.Empty)
            .Generate(5);
        var discordGuildMember = new AutoFaker<GuildMember>()
            .RuleFor(gm => gm.Roles, _ => discordGuildRoles.Select(r => r.ID).ToList())
            .Generate();
        var discordGuild = new AutoFaker<RemoraGuild>()
            .RuleFor(g => g.OwnerID, _ => userId)
            .RuleFor(g => g.ID, spearGuild.Id)
            .Generate();

        _guildApiMock
            .Setup(g => g.GetGuildAsync(spearGuild.Id, It.IsAny<Optional<bool>>(), It.IsAny<CancellationToken>()).Result)
            .Returns(Result<IGuild>.FromSuccess(discordGuild));
        _guildApiMock
            .Setup(g => g.GetGuildRolesAsync(spearGuild.Id, It.IsAny<CancellationToken>()).Result)
            .Returns(Result<IReadOnlyList<IRole>>.FromSuccess(discordGuildRoles));
        _guildApiMock
            .Setup(g => g.GetGuildMemberAsync(spearGuild.Id, It.IsAny<Snowflake>(), It.IsAny<CancellationToken>()).Result)
            .Returns(Result<IGuildMember>.FromSuccess(discordGuildMember));
        _operationContextMock
            .Setup(oc => oc.GuildId)
            .Returns(spearGuild.Id);
        _operationContextMock
            .Setup(oc => oc.UserId)
            .Returns(userId);

        var canModify = await _authorization.InvokerCanModifyAuthroizationAsync(CancellationToken.None);

        canModify.IsSuccess.Should().BeTrue();
        canModify.Entity.Should().BeTrue();
    }
}