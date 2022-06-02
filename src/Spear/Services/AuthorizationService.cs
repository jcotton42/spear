using Microsoft.EntityFrameworkCore;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Contexts;
using Remora.Rest.Core;
using Remora.Results;
using Spear.Models;

namespace Spear.Services;

public class AuthorizationService {
    private record RolesAndPermissions(IReadOnlyList<Snowflake> Roles, IDiscordPermissionSet Permissions);

    private readonly IDiscordRestChannelAPI _channelApi;
    private readonly ICommandContext _commandContext;
    private readonly IDiscordRestGuildAPI _guildApi;
    private readonly SpearContext _spearContext;

    public AuthorizationService(IDiscordRestChannelAPI channelApi, ICommandContext commandContext, IDiscordRestGuildAPI guildApi, SpearContext spearContext) {
        _channelApi = channelApi;
        _commandContext = commandContext;
        _guildApi = guildApi;
        _spearContext = spearContext;
    }

    public Task<Result> GrantDefaultPermissionAsync(Permission permission, CancellationToken ct) =>
        UpsertDefaultPermissionAsync(permission, PermissionMode.Allow, ct);

    public Task<Result> DenyDefaultPermissionAsync(Permission permission, CancellationToken ct) =>
        UpsertDefaultPermissionAsync(permission, PermissionMode.Deny, ct);

    private async Task<Result> UpsertDefaultPermissionAsync(Permission permission, PermissionMode mode, CancellationToken ct) {
        if(!_commandContext.GuildID.IsDefined(out var guildId)) {
            return new InvalidOperationError("Permissions can only be modified from within a guild");
        }

        var entry = await _spearContext.PermissionDefaults.FindAsync(new object[] {guildId, permission}, ct);
        if(entry is null) {
            entry = new PermissionDefault {GuildId = guildId, Permission = permission};
            _spearContext.PermissionDefaults.Add(entry);
        }

        entry.Mode = mode;
        await _spearContext.SaveChangesAsync(ct);

        return Result.FromSuccess();
    }

    public async Task<Result> ClearDefaultPermissionAsync(Permission permission, CancellationToken ct) {
        if(!_commandContext.GuildID.IsDefined(out var guildId)) {
            return new InvalidOperationError("Permissions can only be modified from within a guild");
        }

        var entry = await _spearContext.PermissionDefaults.FindAsync(new object[] {guildId, permission}, ct);
        if(entry is null) {
            return new NotFoundError("No permission default found for this role");
        }

        _spearContext.PermissionDefaults.Remove(entry);
        await _spearContext.SaveChangesAsync(ct);

        return Result.FromSuccess();
    }

    public Task<Result> GrantPermissionAsync(Snowflake roleId, Permission permission, CancellationToken ct) =>
        UpsertPermissionAsync(roleId, permission, PermissionMode.Allow, ct);

    public Task<Result> DenyPermissionAsync(Snowflake roleId, Permission permission, CancellationToken ct) =>
        UpsertPermissionAsync(roleId, permission, PermissionMode.Deny, ct);

    private async Task<Result> UpsertPermissionAsync(Snowflake roleId, Permission permission, PermissionMode mode, CancellationToken ct) {
        if(!_commandContext.GuildID.IsDefined(out var guildId)) {
            return new InvalidOperationError("Permissions can only be modified from within a guild");
        }

        var entry = await _spearContext.PermissionEntries.FindAsync(new object[] {guildId, roleId, permission}, ct);
        if(entry is null) {
            entry = new PermissionEntry {GuildId = guildId, RoleId = roleId, Permission = permission};
            _spearContext.PermissionEntries.Add(entry);
        }

        entry.Mode = mode;
        await _spearContext.SaveChangesAsync(ct);

        return Result.FromSuccess();
    }

    public async Task<Result> ClearPermissionAsync(Snowflake roleId, Permission permission, CancellationToken ct) {
        if(!_commandContext.GuildID.IsDefined(out var guildId)) {
            return new InvalidOperationError("Permissions can only be modified from within a guild");
        }

        var entry = await _spearContext.PermissionEntries.FindAsync(new object[] {guildId, roleId, permission}, ct);
        if(entry is null) {
            return new NotFoundError("No permission entry found for this role");
        }

        _spearContext.PermissionEntries.Remove(entry);
        await _spearContext.SaveChangesAsync(ct);

        return Result.FromSuccess();
    }

    public Task<Result<bool>> InvokerCanSubmitPromptsAsync(CancellationToken ct) =>
        InvokerHasPermissionAsync(Permission.SubmitPrompts, true, ct);

    public async Task<Result<bool>> InvokerCanEditOrDeletePromptsAsync(Prompt prompt, CancellationToken ct) {
        if(_commandContext.User.ID == prompt.Submitter) return true;
        return await InvokerHasPermissionAsync(Permission.ModeratePrompts, false, ct);
    }

    public Task<Result<bool>> InvokerCanModerateBooksAsync(CancellationToken ct) =>
        InvokerHasPermissionAsync(Permission.ModerateBooks, false, ct);

    /// <summary>
    /// Returns whether the invoking user from the interaction context has the given <paramref name="permission"/>.
    /// </summary>
    /// <param name="permission">The <see cref="Permission"/> to check for.</param>
    /// <param name="default">The default if there are no matching permission entries.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>Whether the invoker has that permission.</returns>
    /// <remarks>
    /// <para>
    /// Permission entries with <see cref="PermissionMode.Deny"/> override those with <see cref="PermissionMode.Allow"/>.
    /// </para>
    /// <para>
    /// That is, if the user is part of two roles, one of which is allowed that permission, and the other which is denied
    /// that permission, this method will return false.
    /// </para>
    /// </remarks>
    private async Task<Result<bool>> InvokerHasPermissionAsync(Permission permission, bool @default, CancellationToken ct) {
        if(!_commandContext.GuildID.IsDefined(out var guildId)) {
            return new InvalidOperationError("Commands outside of guilds may not mandate permissions");
        }

        var getGuild = await _guildApi.GetGuildAsync(guildId, ct: ct);
        if(!getGuild.IsDefined(out var guild)) return Result<bool>.FromError(getGuild);
        if(guild.OwnerID == _commandContext.User.ID) return true;

        var get = await GetInvokerRolesAndPermissionsAsync(ct);
        if(!get.IsDefined(out var rp)) return Result<bool>.FromError(get);
        if(rp.Permissions.HasPermission(DiscordPermission.ManageGuild)) return true;

        var defaultPermission = await _spearContext.PermissionDefaults.FindAsync(new object[] {guildId, permission}, ct);
        var permissions = await _spearContext.PermissionEntries
            .Where(pe =>
                pe.Permission == permission
                && pe.GuildId == guildId
                && rp.Roles.Contains(pe.RoleId)
            ).ToListAsync(ct);

        if(permissions.Any()) {
            return permissions.All(p => p.Mode == PermissionMode.Allow);
        }

        if(defaultPermission is not null) return defaultPermission.Mode == PermissionMode.Allow;
        return @default;
    }

    private async Task<Result<RolesAndPermissions>> GetInvokerRolesAndPermissionsAsync(CancellationToken ct) {
        // this implementation is pretty much ripped from Remora.Discord's RequireDiscordPermissionCondition 
        if(!_commandContext.GuildID.IsDefined(out var guildId)) {
            return new InvalidOperationError("Permissions are only applicable to commands executed within a guild.");
        }

        var getMember = await _guildApi.GetGuildMemberAsync(guildId, _commandContext.User.ID, ct);
        if(!getMember.IsDefined(out var member)) return Result<RolesAndPermissions>.FromError(getMember);

        var getGuildRoles = await _guildApi.GetGuildRolesAsync(guildId, ct);
        if(!getGuildRoles.IsDefined(out var guildRoles)) return Result<RolesAndPermissions>.FromError(getGuildRoles);

        var getChannel = await _channelApi.GetChannelAsync(_commandContext.ChannelID, ct);
        if(!getChannel.IsDefined(out var channel)) return Result<RolesAndPermissions>.FromError(getChannel);

        var everyoneRole = guildRoles.First(r => r.ID == guildId);
        var memberRoles = guildRoles.Where(r => member.Roles.Contains(r.ID)).ToList();
        var permissionOverwrites = channel.PermissionOverwrites.HasValue
            ? channel.PermissionOverwrites.Value
            : Array.Empty<PermissionOverwrite>();

        var computedPermissions = DiscordPermissionSet.ComputePermissions(
            _commandContext.User.ID,
            everyoneRole,
            memberRoles,
            permissionOverwrites
        );

        return new RolesAndPermissions(member.Roles, computedPermissions);
    }
}
