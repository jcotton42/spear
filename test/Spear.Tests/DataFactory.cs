using AutoBogus;
using Bogus;
using Remora.Discord.API;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Objects;
using Remora.Rest.Core;
using RemoraGuild = Remora.Discord.API.Objects.Guild;
using SpearGuild = Spear.Models.Guild;

namespace Spear.Tests;

public static class DataFactory {
    private const ulong IdMin = 100000000000000000;
    private const ulong IdMax = 999999999999999999;

    public static Role CreateRole(params DiscordPermission[] permissions) {
        return new AutoFaker<Role>()
            .RuleFor(r => r.ID, f => DiscordSnowflake.New(f.Random.ULong(IdMin, IdMax)))
            .RuleFor(r => r.Permissions, _ => new DiscordPermissionSet(permissions))
            .RuleFor(r => r.Name, f => f.Name.JobTitle())
            .Generate();
    }

    public static GuildMember CreateGuildMember(IEnumerable<Role> roles) {
        return new AutoFaker<GuildMember>()
            .RuleFor(gm => gm.Roles, _ => roles.Select(r => r.ID).ToList().AsReadOnly())
            .RuleFor(gm => gm.Nickname, f => f.Internet.UserName())
            .Generate();
    }

    public static RemoraGuild CreateRemoraGuild(Snowflake? ownerId = null) {
        return new AutoFaker<RemoraGuild>()
            .RuleFor(g => g.ID, f => DiscordSnowflake.New(f.Random.ULong(IdMin, IdMax)))
            .RuleFor(g => g.OwnerID, f => ownerId ?? DiscordSnowflake.New(f.Random.ULong(IdMin, IdMax)))
            .RuleFor(g => g.Name, f => f.Company.CompanyName())
            .Generate();
    }

    public static SpearGuild CreateSpearGuild(RemoraGuild remoraGuild) {
        return new SpearGuild {
            Id = remoraGuild.ID,
            Name = remoraGuild.Name,
        };
    }

    public static Snowflake CreateUserId() =>
        DiscordSnowflake.New(new Faker().Random.ULong(IdMin, IdMax));
}
