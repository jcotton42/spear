using Microsoft.EntityFrameworkCore;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.Commands.Contexts;
using Remora.Rest.Core;
using Remora.Results;
using Spear.Models;

namespace Spear.Services;

public class AuthorizationService {
    private readonly InteractionContext _interactionContext;
    private readonly SpearContext _spearContext;

    public AuthorizationService(InteractionContext interactionContext, SpearContext spearContext) {
        _interactionContext = interactionContext;
        _spearContext = spearContext;
    }

    public Task<Result> GrantDefaultPermissionAsync(Permission permission, CancellationToken ct) =>
        UpsertDefaultPermissionAsync(permission, PermissionMode.Allow, ct);

    public Task<Result> DenyDefaultPermissionAsync(Permission permission, CancellationToken ct) =>
        UpsertDefaultPermissionAsync(permission, PermissionMode.Deny, ct);

    private async Task<Result> UpsertDefaultPermissionAsync(Permission permission, PermissionMode mode, CancellationToken ct) {
        if(!_interactionContext.GuildID.IsDefined(out var guildId)) {
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
        if(!_interactionContext.GuildID.IsDefined(out var guildId)) {
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
        if(!_interactionContext.GuildID.IsDefined(out var guildId)) {
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
        if(!_interactionContext.GuildID.IsDefined(out var guildId)) {
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

    public Task<bool> InvokerCanSubmitPromptsAsync(CancellationToken ct) =>
        InvokerHasPermissionAsync(Permission.SubmitPrompts, true, ct);

    public async Task<bool> InvokerCanEditOrDeletePromptsAsync(Prompt prompt, CancellationToken ct) =>
        _interactionContext.User.ID == prompt.Submitter || await InvokerHasPermissionAsync(Permission.ModeratePrompts, false, ct);

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
    private async Task<bool> InvokerHasPermissionAsync(Permission permission, bool @default, CancellationToken ct) {
        if(!_interactionContext.GuildID.IsDefined(out var guildId)) return false;

        if(!_interactionContext.Member.IsDefined(out var member)) return false;
        if(member.Permissions.IsDefined(out var guildMemberPermissions)
           && guildMemberPermissions.HasPermission(DiscordPermission.ManageGuild)) return true;

        var defaultPermission = await _spearContext.PermissionDefaults.FindAsync(new object[] {guildId, permission}, ct);
        var permissions = await _spearContext.PermissionEntries
            .Where(pe =>
                pe.Permission == permission
                && pe.GuildId == guildId
                && member.Roles.Contains(pe.RoleId)
            ).ToListAsync(ct);

        if(!permissions.Any()) {
            if(defaultPermission is not null) return defaultPermission.Mode == PermissionMode.Allow;
            return @default;
        }
        return permissions.All(p => p.Mode == PermissionMode.Allow);
    }
}
