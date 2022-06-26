using LazyCache;
using Microsoft.EntityFrameworkCore;
using Remora.Rest.Core;
using Remora.Results;
using Spear.Models;

namespace Spear.Services;

public record GuildSettings(Rating SafeChannelRatingCap, Rating NsfwChannelRatingCap);

public class GuildService {
    private static readonly EventId RegisteredGuildEventId = new(1, "RegisteredGuild");
    private static readonly EventId UpdatedGuildEventId = new(2, "UpdatedGuild");

    private readonly IAppCache _cache;
    private readonly ILogger _logger;
    private readonly SpearContext _spearContext;

    public GuildService(IAppCache cache, ILogger<GuildService> logger, SpearContext spearContext) {
        _cache = cache;
        _logger = logger;
        _spearContext = spearContext;
    }

    public async Task<Result<GuildSettings>> GetSettingsAsync(Snowflake id, CancellationToken ct) {
        var settings = await _cache.GetOrAddAsync(CacheKeys.GuildSettings(id), GetGuildSettings);
        if(!settings.HasValue) return new InvalidOperationError("Guild is not registered");
        return settings.Value;

        async Task<Optional<GuildSettings>> GetGuildSettings() {
            return await _spearContext.Guilds
                .Where(guild => guild.Id == id)
                .Select(guild => new GuildSettings(guild.SafeChannelRatingCap, guild.NsfwChannelRatingCap))
                .FirstOrDefaultAsync(ct) is { } s
                ? new Optional<GuildSettings>(s)
                : new Optional<GuildSettings>();
        }
    }

    public async Task<Result> SetSettingsAsync(Snowflake id, GuildSettings settings, CancellationToken ct) {
        var guild = await _spearContext.Guilds.FindAsync(new object[] {id}, ct);
        if(guild is null) return new InvalidOperationError("Guild not registered");

        guild.SafeChannelRatingCap = settings.SafeChannelRatingCap;
        guild.NsfwChannelRatingCap = settings.NsfwChannelRatingCap;
        await _spearContext.SaveChangesAsync(ct);

        _cache.Add(CacheKeys.GuildSettings(id), new GuildSettings(guild.SafeChannelRatingCap, guild.NsfwChannelRatingCap));
        return Result.FromSuccess();
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
