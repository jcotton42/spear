using EntityFramework.Exceptions.Common;
using Microsoft.EntityFrameworkCore;
using Remora.Discord.Commands.Contexts;
using Remora.Results;
using Spear.Models;

namespace Spear.Services;

public class GuildService {
    private readonly ICommandContext _commandContext;
    private readonly SpearContext _spearContext;

    public GuildService(ICommandContext commandContext, SpearContext spearContext) {
        _commandContext = commandContext;
        _spearContext = spearContext;
    }

    public async Task<Result> RegisterGuildAsync(CancellationToken ct) {
        _spearContext.Guilds.Add(new Guild {
            Id = _commandContext.GuildID.Value,
        });

        try {
            await _spearContext.SaveChangesAsync(ct);
        } catch(UniqueConstraintException) {
            return new InvalidOperationError("This guild is already registered");
        }

        return Result.FromSuccess();
    }

    public async Task<bool> IsGuildRegisteredAsync(CancellationToken ct) {
        var guild = await _spearContext.Guilds
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.Id == _commandContext.GuildID.Value, ct);
        return guild is not null;
    }
}
