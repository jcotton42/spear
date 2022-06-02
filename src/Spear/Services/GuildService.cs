using Microsoft.EntityFrameworkCore;
using Remora.Rest.Core;
using Spear.Models;

namespace Spear.Services;

public class GuildService {
    private static readonly EventId RegisteredGuildEventId = new(1, "RegisteredGuild");
    private static readonly EventId UpdatedGuildEventId = new(2, "UpdatedGuild");

    private readonly ILogger _logger;
    private readonly SpearContext _spearContext;

    public GuildService(ILogger<GuildService> logger, SpearContext spearContext) {
        _logger = logger;
        _spearContext = spearContext;
    }

    public async Task UpsertGuildAsync(Snowflake id, string name, CancellationToken ct) {
        var guild = await _spearContext.Guilds.FindAsync(new object[] {id}, ct);
        if(guild is null) {
            guild = new Guild {Id = id, Name = name};
            _spearContext.Guilds.Add(guild);
            await _spearContext.SaveChangesAsync(ct);
            _logger.LogInformation(RegisteredGuildEventId, "Registered guild {Name} ({Id})", name, id);
        } else if(guild.Name != name) {
            guild.Name = name;
            await _spearContext.SaveChangesAsync(ct);
            _logger.LogInformation(UpdatedGuildEventId, "Updated guild {Name} ({Id})", name, id);
        }
    }

    public async Task<bool> IsGuildRegisteredAsync(Snowflake id, CancellationToken ct) {
        var guild = await _spearContext.Guilds
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.Id == id, ct);
        return guild is not null;
    }
}
