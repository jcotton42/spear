using Remora.Commands.Conditions;
using Remora.Results;
using Spear.Conditions.Attributes;
using Spear.Services;

namespace Spear.Conditions;

public class RequireRegisteredGuildCondition : ICondition<RequireRegisteredGuildAttribute> {
    private readonly GuildService _guild;

    public RequireRegisteredGuildCondition(GuildService guild) {
        _guild = guild;
    }

    public async ValueTask<Result> CheckAsync(RequireRegisteredGuildAttribute attribute, CancellationToken ct) {
        if(await _guild.IsGuildRegisteredAsync(ct)) return Result.FromSuccess();
        else return new InvalidOperationError("You must register this guild before you can use this command");
    }
}
