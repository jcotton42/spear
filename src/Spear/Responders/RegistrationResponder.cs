using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.Gateway.Responders;
using Remora.Results;
using Spear.Services;

namespace Spear.Responders;

public class RegistrationResponder : IResponder<IGuildCreate> {
    private readonly GuildService _guild;

    public RegistrationResponder(GuildService guild) => _guild = guild;

    public async Task<Result> RespondAsync(IGuildCreate gatewayEvent, CancellationToken ct) {
        await _guild.UpsertGuildAsync(gatewayEvent.ID, gatewayEvent.Name, ct);
        return Result.FromSuccess();
    }
}
