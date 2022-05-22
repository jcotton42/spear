using System.ComponentModel;
using EntityFramework.Exceptions.Common;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.Commands.Conditions;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Feedback.Services;
using Remora.Results;
using Spear.Models;

namespace Spear.Commands;

public class GuildCommands : CommandGroup {
    private readonly ICommandContext _commandContext;
    private readonly FeedbackService _feedback;
    private readonly SpearContext _spearContext;

    public GuildCommands(ICommandContext commandContext, FeedbackService feedback, SpearContext spearContext) {
        _commandContext = commandContext;
        _feedback = feedback;
        _spearContext = spearContext;
    }

    [Command("register")]
    [RequireContext(ChannelContext.Guild)]
    [Description("Registers this server with the bot")]
    public async Task<IResult> RegisterAsync() {
        _spearContext.Guilds.Add(new Guild {
            Id = _commandContext.GuildID.Value
        });

        Result<IReadOnlyList<IMessage>> reply;
        try {
            await _spearContext.SaveChangesAsync(CancellationToken);
            reply = await _feedback.SendContextualSuccessAsync("I've registered your server!", ct: CancellationToken);
        } catch(UniqueConstraintException) {
            reply = await _feedback.SendContextualErrorAsync("Your guild is already registered!", ct: CancellationToken);
        }

        return reply.IsSuccess ? Result.FromSuccess() : Result.FromError(reply);
    }
}
