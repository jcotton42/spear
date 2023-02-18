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

    public TheoryData<Role[]> Roles { get; } = new() {
        new Role[] {}
    };

    public AuthorizationServiceTests(ServicesFixture servicesFixture) : base(servicesFixture) {
        _authorization = _scope.ServiceProvider.GetRequiredService<AuthorizationService>();
        _channelApiMock = _scope.ServiceProvider.GetRequiredService<Mock<IDiscordRestChannelAPI>>();
        _dbContext = _scope.ServiceProvider.GetRequiredService<SpearContext>();
        _guildApiMock = _scope.ServiceProvider.GetRequiredService<Mock<IDiscordRestGuildAPI>>();
        _operationContextMock = _scope.ServiceProvider.GetRequiredService<Mock<ISpearOperationContext>>();
    }

    [Fact]
    public async Task HasSpearPermissionCheck() {
        var userId = DataFactory.CreateUserId();
        var discordGuildRoles = new Role[] {
            DataFactory.CreateRole(),
            DataFactory.CreateRole(),
            DataFactory.CreateRole(),
        };
        var discordGuildMember = DataFactory.CreateGuildMember(discordGuildRoles);
        var discordGuild = DataFactory.CreateRemoraGuild(ownerId: userId);
        var spearGuild = DataFactory.CreateSpearGuild(discordGuild);

        _guildApiMock
            .Setup(g => g.GetGuildAsync(spearGuild.Id, It.IsAny<Optional<bool>>(), It.IsAny<CancellationToken>()).Result)
            .Returns(Result<IGuild>.FromSuccess(discordGuild));
        _guildApiMock
            .Setup(g => g.GetGuildRolesAsync(spearGuild.Id, It.IsAny<CancellationToken>()).Result)
            .Returns(Result<IReadOnlyList<IRole>>.FromSuccess(discordGuildRoles));
        _guildApiMock
            .Setup(g => g.GetGuildMemberAsync(spearGuild.Id, userId, It.IsAny<CancellationToken>()).Result)
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
