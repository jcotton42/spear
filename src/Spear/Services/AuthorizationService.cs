using Microsoft.EntityFrameworkCore;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Extensions;
using Remora.Discord.Commands.Results;
using Remora.Rest.Core;
using Remora.Results;
using Spear.Models;

namespace Spear.Services;

public class AuthorizationService {
    private record RolesAndPermissions(IReadOnlyList<Snowflake> Roles, IDiscordPermissionSet Permissions);

    private readonly IDiscordRestChannelAPI _channelApi;
    private readonly IDiscordRestGuildAPI _guildApi;
    private readonly ISpearOperationContext _operationContext;
    private readonly SpearContext _spearContext;

    public AuthorizationService(IDiscordRestChannelAPI channelApi, IDiscordRestGuildAPI guildApi,
        ISpearOperationContext operationContext, SpearContext spearContext) {
        _channelApi = channelApi;
        _guildApi = guildApi;
        _operationContext = operationContext;
        _spearContext = spearContext;
    }

    public Task<Result> GrantDefaultPermissionAsync(Permission permission, CancellationToken ct) =>
        UpsertDefaultPermissionAsync(permission, PermissionMode.Allow, ct);

    public Task<Result> DenyDefaultPermissionAsync(Permission permission, CancellationToken ct) =>
        UpsertDefaultPermissionAsync(permission, PermissionMode.Deny, ct);

    private async Task<Result> UpsertDefaultPermissionAsync(Permission permission, PermissionMode mode, CancellationToken ct) {
        var getCanModify = await InvokerCanModifyAuthroizationAsync(ct);
        if(!getCanModify.IsDefined(out var canModify)) return Result.FromError(getCanModify);
        if(!canModify) return new PermissionDeniedError("You do not have permission to modify authorization.", DiscordPermission.ManageGuild);

        var entry = await _spearContext.PermissionDefaults.FindAsync(new object[] { _operationContext.GuildId, permission }, ct);
        if(entry is null) {
            entry = new PermissionDefault { GuildId = _operationContext.GuildId, Permission = permission };
            _spearContext.PermissionDefaults.Add(entry);
        }

        entry.Mode = mode;
        await _spearContext.SaveChangesAsync(ct);

        return Result.FromSuccess();
    }

    public async Task<Result> ClearDefaultPermissionAsync(Permission permission, CancellationToken ct) {
        var getCanModify = await InvokerCanModifyAuthroizationAsync(ct);
        if(!getCanModify.IsDefined(out var canModify)) return Result.FromError(getCanModify);
        if(!canModify) return new PermissionDeniedError("You do not have permission to modify authorization.", DiscordPermission.ManageGuild);

        var entry = await _spearContext.PermissionDefaults.FindAsync(new object[] { _operationContext.GuildId, permission }, ct);
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
        var getCanModify = await InvokerCanModifyAuthroizationAsync(ct);
        if(!getCanModify.IsDefined(out var canModify)) return Result.FromError(getCanModify);
        if(!canModify) return new PermissionDeniedError("You do not have permission to modify authorization.", DiscordPermission.ManageGuild);

        var entry = await _spearContext.PermissionEntries.FindAsync(new object[] { _operationContext.GuildId, roleId, permission }, ct);
        if(entry is null) {
            entry = new PermissionEntry { GuildId = _operationContext.GuildId, RoleId = roleId, Permission = permission };
            _spearContext.PermissionEntries.Add(entry);
        }

        entry.Mode = mode;
        await _spearContext.SaveChangesAsync(ct);

        return Result.FromSuccess();
    }

    public async Task<Result> ClearPermissionAsync(Snowflake roleId, Permission permission, CancellationToken ct) {
        var getCanModify = await InvokerCanModifyAuthroizationAsync(ct);
        if(!getCanModify.IsDefined(out var canModify)) return Result.FromError(getCanModify);
        if(!canModify) return new PermissionDeniedError("You do not have permission to modify authorization.", DiscordPermission.ManageGuild);

        var entry = await _spearContext.PermissionEntries.FindAsync(new object[] { _operationContext.GuildId, roleId, permission }, ct);
        if(entry is null) {
            return new NotFoundError("No permission entry found for this role");
        }

        _spearContext.PermissionEntries.Remove(entry);
        await _spearContext.SaveChangesAsync(ct);

        return Result.FromSuccess();
    }

    public Task<Result<bool>> InvokerCanSubmitPromptsAsync(CancellationToken ct) =>
        InvokerHasSpearPermissionAsync(Permission.SubmitPrompts, true, ct);

    public async Task<Result<bool>> InvokerCanEditOrDeletePromptsAsync(Prompt prompt, CancellationToken ct) {
        if(_operationContext.UserId == prompt.Submitter) return true;
        return await InvokerHasSpearPermissionAsync(Permission.ModeratePrompts, false, ct);
    }

    public Task<Result<bool>> InvokerCanModerateBooksAsync(CancellationToken ct) =>
        InvokerHasSpearPermissionAsync(Permission.ModerateBooks, false, ct);

    public Task<Result<bool>> InvokerCanModifyAuthroizationAsync(CancellationToken ct) =>
        InvokerHasDiscordPermissionAsync(DiscordPermission.ManageGuild, ct);

    private async Task<Result<bool>> InvokerHasDiscordPermissionAsync(DiscordPermission permission, CancellationToken ct) {
        var getGuild = await _guildApi.GetGuildAsync(_operationContext.GuildId, ct: ct);
        if(!getGuild.IsDefined(out var guild)) return Result<bool>.FromError(getGuild);
        if(guild.OwnerID == _operationContext.UserId) return true;

        var getRp = await GetInvokerRolesAndPermissionsAsync(ct);
        if(!getRp.IsDefined(out var rp)) return Result<bool>.FromError(getRp);
        return rp.Permissions.HasPermission(permission);
    }

    /// <summary>
    /// Returns whether the invoking user from the operation context has the given <paramref name="permission"/>.
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
    /// <para>
    /// Users that are the guild's owner, or have <see cref="DiscordPermission.ManageGuild"/> in the server will always
    /// return true.
    /// </para>
    /// </remarks>
    private async Task<Result<bool>> InvokerHasSpearPermissionAsync(Permission permission, bool @default, CancellationToken ct) {
        var getGuild = await _guildApi.GetGuildAsync(_operationContext.GuildId, ct: ct);
        if(!getGuild.IsDefined(out var guild)) return Result<bool>.FromError(getGuild);
        if(guild.OwnerID == _operationContext.UserId) return true;

        var get = await GetInvokerRolesAndPermissionsAsync(ct);
        if(!get.IsDefined(out var rp)) return Result<bool>.FromError(get);
        if(rp.Permissions.HasPermission(DiscordPermission.ManageGuild)) return true;

        var defaultPermission = await _spearContext.PermissionDefaults.FindAsync(new object[] { _operationContext.GuildId, permission }, ct);
        var permissions = await _spearContext.PermissionEntries
            .Where(pe =>
                pe.Permission == permission
                && pe.GuildId == _operationContext.GuildId
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
        var getMember = await _guildApi.GetGuildMemberAsync(_operationContext.GuildId, _operationContext.UserId, ct);
        if(!getMember.IsDefined(out var member)) return Result<RolesAndPermissions>.FromError(getMember);

        var getGuildRoles = await _guildApi.GetGuildRolesAsync(_operationContext.GuildId, ct);
        if(!getGuildRoles.IsDefined(out var guildRoles)) return Result<RolesAndPermissions>.FromError(getGuildRoles);

        // TODO, this actually can be optional
        if(!_operationContext.ChannelId.HasValue) {
            return new InvalidOperationError("Permissions require a channel context");
        }

        var getChannel = await _channelApi.GetChannelAsync(_operationContext.ChannelId.Value, ct);
        if(!getChannel.IsDefined(out var channel)) return Result<RolesAndPermissions>.FromError(getChannel);

        var everyoneRole = guildRoles.First(r => r.ID == _operationContext.GuildId);
        var memberRoles = guildRoles.Where(r => member.Roles.Contains(r.ID)).ToList();
        var permissionOverwrites = channel.PermissionOverwrites.HasValue
            ? channel.PermissionOverwrites.Value
            : Array.Empty<PermissionOverwrite>();

        var computedPermissions = DiscordPermissionSet.ComputePermissions(
            _operationContext.UserId,
            everyoneRole,
            memberRoles,
            permissionOverwrites
        );

        return new RolesAndPermissions(member.Roles, computedPermissions);
    }
}
