using System.ComponentModel;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Feedback.Services;
using Remora.Results;

namespace Spear.Commands;

public class PromptCommands : CommandGroup {
    private readonly ICommandContext _commandContext;
    private readonly FeedbackService _feedback;

    public PromptCommands(ICommandContext commandContext, FeedbackService feedback) {
        _commandContext = commandContext;
        _feedback = feedback;
    }

    [Command("suggest")]
    [Description("Add a suggestion to the prompt list")]
    public async Task<IResult> SuggestAsync(
        [Description(@"The suggestion. Use `\n` for line breaks.")] string suggestion
    ) {
        suggestion = suggestion.Replace("\\n", "\n");

        var reply = await _feedback.SendContextualSuccessAsync(
            $"{_commandContext.User.ID} That's an excellent suggestion! I'll add it to the list\n#1\n\n>>> {suggestion}",
            ct: CancellationToken
        );

        return reply.IsSuccess ? Result.FromSuccess() : Result.FromError(reply);
    }
}
