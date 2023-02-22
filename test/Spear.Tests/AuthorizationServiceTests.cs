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
using SpearGuild = Spear.Models.Guild;

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
        const ulong IdMin = 100000000000000000;
        const ulong IdMax = 999999999999999999;
        var f = new Faker();
        var ownerId = DiscordSnowflake.New(f.Random.ULong(IdMin, IdMax));
        var userId = DiscordSnowflake.New(f.Random.ULong(IdMin, IdMax));
        var channelId = DiscordSnowflake.New(f.Random.ULong(IdMin, IdMax));

        var spearGuild = new SpearGuild {
            Id = DiscordSnowflake.New(f.Random.ULong(IdMin, IdMax)),
            Name = f.Company.CompanyName(),
            PermissionEntries = new List<PermissionEntry> {
                new() {
                    RoleId = DiscordSnowflake.New(f.Random.ULong(IdMin, IdMax)),
                    Permission = Permission.SubmitPrompts,
                    Mode = PermissionMode.Allow,
                },
                new() {
                    RoleId = DiscordSnowflake.New(f.Random.ULong(IdMin, IdMax)),
                    Permission = Permission.SubmitPrompts,
                    Mode = PermissionMode.Allow,
                },
            },
            PermissionDefaults = new List<PermissionDefault> {
                new() {
                    Permission = Permission.SubmitPrompts,
                    Mode = PermissionMode.Allow,
                },
            }
        };

        _dbContext.Guilds.Add(spearGuild);
        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();
        var remoraGuild = Mock.Of<IGuild>(guild =>
            guild.ID == spearGuild.Id
            && guild.OwnerID == ownerId
            && guild.Name == spearGuild.Name);
        // maybe use MockRepository and/or set strict behavior
        var roles = new List<IRole> {
            Mock.Of<IRole>(r =>
                r.ID == spearGuild.Id
                && r.Permissions == DiscordPermissionSet.Empty
                && r.Name == "@everyone"),
            Mock.Of<IRole>(r =>
                r.ID == spearGuild.PermissionEntries[0].RoleId
                && r.Permissions == new DiscordPermissionSet(DiscordPermission.ManageGuild)
                && r.Name == f.Name.JobTitle()),
            Mock.Of<IRole>(r =>
                r.ID == spearGuild.PermissionEntries[1].RoleId
                && r.Permissions == new DiscordPermissionSet(DiscordPermission.ManageGuild)
                && r.Name == f.Name.JobTitle()),
        };
        var member = Mock.Of<IGuildMember>(gm =>
            gm.Roles == roles.Select(r => r.ID).ToList()
            && gm.Nickname == f.Internet.UserName(null, null));
        var channel = Mock.Of<IChannel>(c =>
            c.ID == channelId
            && c.PermissionOverwrites == default);
        _guildApiMock
            .Setup(g => g.GetGuildAsync(spearGuild.Id, It.IsAny<Optional<bool>>(), It.IsAny<CancellationToken>()).Result)
            .Returns(Result<IGuild>.FromSuccess(remoraGuild));
        _guildApiMock
            .Setup(g => g.GetGuildRolesAsync(spearGuild.Id, It.IsAny<CancellationToken>()).Result)
            .Returns(Result<IReadOnlyList<IRole>>.FromSuccess(roles));
        _guildApiMock
            .Setup(g => g.GetGuildMemberAsync(spearGuild.Id, userId, It.IsAny<CancellationToken>()).Result)
            .Returns(Result<IGuildMember>.FromSuccess(member));
        _operationContextMock
            .Setup(oc => oc.GuildId)
            .Returns(spearGuild.Id);
        _operationContextMock
            .Setup(oc => oc.UserId)
            .Returns(userId);
        _operationContextMock
            .Setup(oc => oc.ChannelId)
            .Returns(channelId);
        _channelApiMock
            .Setup(c => c.GetChannelAsync(channel.ID, It.IsAny<CancellationToken>()).Result)
            .Returns(Result<IChannel>.FromSuccess(channel));

        var canModify = await _authorization.InvokerCanModifyAuthroizationAsync(CancellationToken.None);

        canModify.IsSuccess.Should().BeTrue();
        canModify.Entity.Should().BeTrue();
    }

    [Fact]
    public async Task Old_HasSpearPermissionCheck() {
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
