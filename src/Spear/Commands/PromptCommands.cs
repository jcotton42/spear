using System.ComponentModel;
using Microsoft.EntityFrameworkCore;
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

        return await _feedback.SendContextualSuccessAsync(
            $"That's an excellent suggestion! I'll add it to the list\n#`{prompt.Id}`\n>>> {suggestion}",
            ct: CancellationToken
        );
    }

    [Command("editprompt")]
    [RequireContext(ChannelContext.Guild)]
    [Description("Edits a prompt")]
    public async Task<IResult> EditPromptAsync(
        [Description("The prompt's ID")] int id,
        [Description(@"The new prompt. Use `\n` for line breaks")] string newSuggestion
    ) {
        newSuggestion = newSuggestion.Replace("\\n", "\n");
        var prompt = await _spearContext.Prompts
            .FirstOrDefaultAsync(p => p.Id == id && p.GuildId == _commandContext.GuildID.Value, CancellationToken);
        if(prompt is null) {
            return Result.FromError(new NotFoundError($"No prompt found with ID {id}"));
        }

        // TODO check permissions (submitter can always edit, as well as anyone with Manage Guild)
        prompt.Text = newSuggestion;
        await _spearContext.SaveChangesAsync(CancellationToken);

        return await _feedback.SendContextualSuccessAsync(
            $"I have updated the suggestion.\n>>> {newSuggestion}",
            ct: CancellationToken
        );
    }

    [Command("deleteprompt")]
    [RequireContext(ChannelContext.Guild)]
    [Description("Deletes a prompt")]
    public async Task<IResult> DeletePromptAsync(
        [Description("The prompt's ID")] int id
    ) {
        var prompt = await _spearContext.Prompts
            .FirstOrDefaultAsync(p => p.Id == id && p.GuildId == _commandContext.GuildID.Value, CancellationToken);
        if(prompt is null) {
            return Result.FromError(new NotFoundError($"No prompt found with ID {id}"));
        }

        // TODO check permissions
        _spearContext.Prompts.Remove(prompt);
        await _spearContext.SaveChangesAsync(CancellationToken);

        return await _feedback.SendContextualSuccessAsync(
            "I have deleted the prompt.",
            ct: CancellationToken
        );
    }

    [Command("prompt")]
    [RequireContext(ChannelContext.Guild)]
    [Description("Gets a random prompt")]
    public async Task<IResult> PromptAsync() {
        var ids = await _spearContext.Prompts
            .Where(p => p.GuildId == _commandContext.GuildID.Value)
            .Select(p => p.Id)
            .ToListAsync(CancellationToken);
        if(!ids.Any()) {
            return Result.FromError(new NotFoundError("No prompts are available for this guild."));
        }

        var id = ids[Random.Shared.Next(ids.Count)];
        var prompt = await _spearContext.Prompts.SingleAsync(p => p.Id == id, CancellationToken);
        var creditLine = prompt.Submitter is not null ? $" by <@{prompt.Submitter}>" : "";

        return await _feedback.SendContextualSuccessAsync(
            $"May I suggest the following{creditLine}?\n#`{prompt.Id}`\n >>> {prompt.Text}",
            ct: CancellationToken
        );
    }
}
