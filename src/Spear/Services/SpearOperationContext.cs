using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Extensions;
using Remora.Discord.Commands.Services;
using Remora.Rest.Core;

namespace Spear.Services;

public interface ISpearOperationContext {
    Snowflake? ChannelId { get; }
    Snowflake GuildId { get; }
    Snowflake UserId { get; }
}

public class SpearDiscordOperationConext : ISpearOperationContext {
    private readonly ContextInjectionService _context;

    public SpearDiscordOperationConext(ContextInjectionService context) => _context = context;

    public Snowflake? ChannelId {
        get {
            var context = _context.Context ?? throw new InvalidOperationException("No context has been set for this scope");
            return context.TryGetChannelID(out var channelId) ? channelId.Value : null;
        }
    }

    public Snowflake GuildId {
        get {
            var context = _context.Context ?? throw new InvalidOperationException("No context has been set for this scope");
            return context.TryGetGuildID(out var guildId)
                ? guildId.Value
                : throw new InvalidOperationException("No channel is present in this context");
        }
    }

    public Snowflake UserId {
        get {
            var context = _context.Context ?? throw new InvalidOperationException("No context has been set for this scope");
            return context.TryGetUserID(out var userId)
                ? userId.Value
                : throw new InvalidOperationException("No user is present in this context");
        }
    }
}
