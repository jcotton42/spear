using System.ComponentModel;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.Commands.Conditions;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Feedback.Services;
using Remora.Results;
using Spear.Conditions.Attributes;
using Spear.Models;
using Spear.Services;

namespace Spear.Commands;

public partial class OldMan {
    [Group("auth")]
    [RequireContext(ChannelContext.Guild)]
    [RequireDiscordPermission(DiscordPermission.ManageGuild)]
    [RequireRegisteredGuild]
    public class AuthorizationCommands : CommandGroup {
        private readonly AuthorizationService _authorization;
        private readonly ICommandContext _commandContext;
        private readonly FeedbackService _feedback;

        public AuthorizationCommands(AuthorizationService authorization, ICommandContext commandContext,
            FeedbackService feedback) {
            _authorization = authorization;
            _commandContext = commandContext;
            _feedback = feedback;
        }

        [Command("grant")]
        [Description("Grants a permission to a role")]
        public async Task<IResult> GrantAsync(
            [Description("The permission to grant")]
            Permission permission,
            [Description("The role to grant the permission to. Use @everyone to set the default")]
            IRole role
        ) {
            IResult grant;
            string message;
            if(role.ID == _commandContext.GuildID.Value) {
                // @everyone passed for role
                grant = await _authorization.GrantDefaultPermissionAsync(permission, CancellationToken);
                message = $"Everyone now has {permission} granted by default";
            }
            else {
                grant = await _authorization.GrantPermissionAsync(role.ID, permission, CancellationToken);
                message = $"Granted {permission} for <@&{role.ID}>";
            }

            if(!grant.IsSuccess) return grant;

            return await _feedback.SendContextualSuccessAsync(message, ct: CancellationToken);
        }

        [Command("deny")]
        [Description("Denies a permission to a role")]
        public async Task<IResult> DenyAsync(
            [Description("The permission to deny")]
            Permission permission,
            [Description("The role to deny the permission to. Use @everyone to set the default")]
            IRole role
        ) {
            IResult deny;
            string message;
            if(role.ID == _commandContext.GuildID.Value) {
                // @everyone passed for role
                deny = await _authorization.DenyDefaultPermissionAsync(permission, CancellationToken);
                message = $"Everyone now has {permission} denied by default";
            }
            else {
                deny = await _authorization.DenyPermissionAsync(role.ID, permission, CancellationToken);
                message = $"Denied {permission} for <@&{role.ID}>";
            }

            if(!deny.IsSuccess) return deny;

            return await _feedback.SendContextualSuccessAsync(message, ct: CancellationToken);
        }

        [Command("reset")]
        [Description("Resets a permission entry")]
        public async Task<IResult> ResetAsync(
            [Description("The permission to reset")]
            Permission permission,
            [Description("The role to reset the permission entry from. Use @everyone to set the default")]
            IRole role
        ) {
            IResult reset;
            string message;
            if(role.ID == _commandContext.GuildID.Value) {
                // @everyone passed for role
                reset = await _authorization.ClearDefaultPermissionAsync(permission, CancellationToken);
                message = $"Reset {permission} for everyone";
            }
            else {
                reset = await _authorization.ClearPermissionAsync(role.ID, permission, CancellationToken);
                message = $"Reset {permission} for <@&{role.ID}>";
            }

            if(!reset.IsSuccess) return reset;

            return await _feedback.SendContextualSuccessAsync(message, ct: CancellationToken);
        }
    }
}
