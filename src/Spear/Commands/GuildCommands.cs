using System.ComponentModel;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.Commands.Conditions;
using Remora.Discord.Commands.Feedback.Services;
using Remora.Results;
using Spear.Services;

namespace Spear.Commands;

[RequireContext(ChannelContext.Guild)]
[RequireDiscordPermission(DiscordPermission.ManageGuild)]
public class GuildCommands : CommandGroup {
    private readonly FeedbackService _feedback;
    private readonly GuildService _guild;

    public GuildCommands(FeedbackService feedback, GuildService guild) {
        _feedback = feedback;
        _guild = guild;
    }

    [Command("register")]
    [Description("Registers this server with the bot")]
    public async Task<IResult> RegisterAsync() {
        var register = await _guild.RegisterGuildAsync(CancellationToken);
        if(!register.IsSuccess) return register;

        return await _feedback.SendContextualSuccessAsync("I've registered your server!", ct: CancellationToken);
    }
}
