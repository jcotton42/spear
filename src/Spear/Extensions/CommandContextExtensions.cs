using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Extensions;
using Remora.Rest.Core;

namespace Spear.Extensions;

public static class CommandContextExtensions {
    public static Snowflake GetChannelId(this ICommandContext context) {
        if(context.TryGetChannelID(out var channelId)) return channelId.Value;
        throw new InvalidOperationException("You need to use the Try version here.");
    }

    public static Snowflake GetGuildId(this ICommandContext context) {
        if(context.TryGetGuildID(out var guildId)) return guildId.Value;
        throw new InvalidOperationException("You need to use the Try version here.");
    }

    public static Snowflake GetUserId(this ICommandContext context) {
        if(context.TryGetUserID(out var userId)) return userId.Value;
        throw new InvalidOperationException("You need to use the Try version here.");
    }
}
