using System.Collections.Concurrent;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Gateway.Responders;
using Remora.Rest.Core;
using Remora.Results;
using Spear.Models;

namespace Spear.Services;

public record Context(
    Snowflake GuildId,
    Snowflake UserId,
    Rating EffectiveRatingCap
);

// singleton
public class ContextStorageService {
    private readonly ConcurrentDictionary<Snowflake, Context> _contexts = new();

    public void InsertContext(Snowflake id, Context context) {
        if(!_contexts.TryAdd(id, context)) {
            // TODO handle the ID already existing somehow
        }
    }

    public Context GetContext(Snowflake id) {
        if(!_contexts.TryGetValue(id, out var context)) {
            // TODO handle context not existing
        }

        return context;
    }

    public void EvictContext(Snowflake id) {
        _ = _contexts.TryRemove(id, out _);
    }
}

// scoped
public class ContextService {
    public Context Context { get; }

    public ContextService(ICommandContext commandContext, ContextStorageService contextStorage) {
        var id = commandContext switch {
            InteractionContext ic => ic.ID,
            MessageContext mc => mc.MessageID,
            { } unknown => throw new InvalidOperationException($"Unsupported Remora context type {unknown.GetType().FullName}"),
        };

        Context = contextStorage.GetContext(id);
    }
}

// early responder
public class ContextInserter : IResponder<IInteractionCreate>, IResponder<IMessageCreate>, IResponder<IMessageUpdate> {
    private readonly IDiscordRestChannelAPI _channelApi;
    private readonly ContextStorageService _contextStorage;
    private readonly GuildService _guild;

    public ContextInserter(IDiscordRestChannelAPI channelApi, ContextStorageService contextStorage, GuildService guild) {
        _channelApi = channelApi;
        _contextStorage = contextStorage;
        _guild = guild;
    }

    public async Task<Result> RespondAsync(IInteractionCreate gatewayEvent, CancellationToken ct) {
        if(!gatewayEvent.GuildID.IsDefined(out var guildId)) {
            return new InvalidOperationError("I don't work in DMs. Sorry!");
        }
        var userId = gatewayEvent switch {
            {User.HasValue: true} => gatewayEvent.User.Value.ID,
            {Member.HasValue: true} => gatewayEvent.Member.Value.User.Value.ID,
            // TODO this exception is not helpful
            _ => throw new InvalidOperationException("Could not determine user on interaction create event"),
        };
        
        var getChannel = await _channelApi.GetChannelAsync(gatewayEvent.ChannelID)
    }

    public Task<Result> RespondAsync(IMessageCreate gatewayEvent, CancellationToken ct) {
        throw new NotImplementedException();
    }

    public Task<Result> RespondAsync(IMessageUpdate gatewayEvent, CancellationToken ct) {
        throw new NotImplementedException();
    }
}
