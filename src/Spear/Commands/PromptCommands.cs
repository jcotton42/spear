using System.ComponentModel;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.Commands.Conditions;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Feedback.Services;
using Remora.Results;
using Spear.Models;

namespace Spear.Commands;

public class PromptCommands : CommandGroup {
    private readonly ICommandContext _commandContext;
    private readonly FeedbackService _feedback;
    private readonly SpearContext _spearContext;

    public PromptCommands(ICommandContext commandContext, FeedbackService feedback, SpearContext spearContext) {
        _commandContext = commandContext;
        _feedback = feedback;
        _spearContext = spearContext;
    }

    [Command("suggest")]
    [RequireContext(ChannelContext.Guild)]
    [Description("Add a suggestion to the prompt list")]
    public async Task<IResult> SuggestAsync(
        [Description(@"The suggestion. Use `\n` for line breaks.")] string suggestion
    ) {
        suggestion = suggestion.Replace("\\n", "\n");
        var prompt = new Prompt {
            GuildId = _commandContext.GuildID.Value,
            Submitter = _commandContext.User.ID,
            Text = suggestion,
        };

        _spearContext.Prompts.Add(prompt);
        await _spearContext.SaveChangesAsync(CancellationToken);

        var reply = await _feedback.SendContextualSuccessAsync(
            $"That's an excellent suggestion! I'll add it to the list\n#`{prompt.Id}`\n\n>>> {suggestion}",
            ct: CancellationToken
        );

        return reply.IsSuccess ? Result.FromSuccess() : Result.FromError(reply);
    }
}
