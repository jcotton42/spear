using System.ComponentModel;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Conditions;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Feedback.Messages;
using Remora.Discord.Commands.Feedback.Services;
using Remora.Discord.Interactivity;
using Remora.Discord.Pagination.Extensions;
using Remora.Results;
using Spear.Extensions;
using Spear.Services;

namespace Spear.Commands;

public partial class OldMan {
    [RequireContext(ChannelContext.Guild)]
    public class PromptCommands : CommandGroup {
        private readonly ICommandContext _commandContext;
        private readonly FeedbackService _feedback;
        private readonly PromptService _prompt;

        public PromptCommands(ICommandContext commandContext,
            FeedbackService feedback, PromptService prompt) {
            _commandContext = commandContext;
            _feedback = feedback;
            _prompt = prompt;
        }

        [Command("suggest")]
        [Description("Add a suggestion to the prompt list")]
        public async Task<IResult> SuggestAsync(
            [Description(@"The suggestion. Use `\n` for line breaks.")]
            [Greedy]
            string suggestion
        ) {
            suggestion = suggestion.Replace("\\n", "\n");
            var result = await _prompt.AddPromptAsync(suggestion, CancellationToken);
            if(!result.IsDefined(out var id)) return result;

            return await _feedback.SendContextualSuccessAsync(
                $"That's an excellent suggestion! I'll add it to the list\n#`{id}`\n>>> {suggestion}",
                ct: CancellationToken
            );
        }

        [Command("editprompt")]
        [Description("Edits a prompt")]
        public async Task<IResult> EditPromptAsync(
            [Description("The prompt's ID")] int id,
            [Description(@"The new prompt. Use `\n` for line breaks")]
            [Greedy]
            string newSuggestion
        ) {
            newSuggestion = newSuggestion.Replace("\\n", "\n");
            var edit = await _prompt.EditPromptAsync(id, newSuggestion, CancellationToken);
            if(!edit.IsSuccess) return edit;

            return await _feedback.SendContextualSuccessAsync(
                $"I have updated the suggestion.\n>>> {newSuggestion}",
                ct: CancellationToken
            );
        }

        [Command("deleteprompt")]
        [Description("Deletes a prompt")]
        public async Task<IResult> DeletePromptAsync(
            [Description("The prompt's ID")] int id
        ) {
            var getPrompt = await _prompt.GetPromptByIdAsync(id, CancellationToken);
            if(!getPrompt.IsDefined(out var prompt)) return getPrompt;

            var embed = new Embed(Description: $"Are you sure you want to delete this prompt?\n>>> {prompt.Text}");
            var options = new FeedbackMessageOptions(
                MessageFlags: MessageFlags.Ephemeral,
                MessageComponents: new[] {
                    new ActionRowComponent(new[] {
                        new ButtonComponent(
                            Label: "Delete",
                            Style: ButtonComponentStyle.Danger,
                            CustomID: CustomIDHelpers.CreateButtonIDWithState(PromptDeletePrompt.Delete, id.ToString())
                        ),
                        new ButtonComponent(
                            Label: "Don't delete",
                            Style: ButtonComponentStyle.Secondary,
                            CustomID: CustomIDHelpers.CreateButtonIDWithState(PromptDeletePrompt.DontDelete, id.ToString())
                        ),
                    })
                }
            );

            return await _feedback.SendContextualEmbedAsync(embed, options, CancellationToken);
        }

        [Command("prompt")]
        [Description("Gets a random prompt")]
        public async Task<IResult> PromptAsync() {
            var get = await _prompt.GetRandomPromptAsync(CancellationToken);
            if(!get.IsDefined(out var prompt)) return get;

            var creditLine = prompt.Submitter is not null ? $" by <@{prompt.Submitter}>" : "";

            return await _feedback.SendContextualNeutralAsync(
                $"May I suggest the following{creditLine}?\n#`{prompt.Id}`\n>>> {prompt.Text}",
                ct: CancellationToken
            );
        }

        [Command("searchprompts")]
        [Description("Searches for a prompt")]
        public async Task<IResult> SearchPromptsAsync(
            [Description("The search term")]
            [Greedy]
            string searchTerm
        ) {
            var search = await _prompt.SearchForPromptsAsync(searchTerm, 25, CancellationToken);
            if(!search.IsDefined(out var prompts)) return search;

            var pages = prompts.Select(p => {
                var creditLine = p.Submitter is not null ? $" (by <@{p.Submitter}>)" : "";
                return new Embed(
                    Description: $"Is this what you were looking for{creditLine}?\n#`{p.Id}`\n>>> {p.Text}");
            }).ToList();

            return await _feedback.SendContextualPaginatedMessageAsync(
                _commandContext.GetUserId(),
                pages,
                ct: CancellationToken
            );
        }
    }
}

public class PromptDeletePrompt : InteractionGroup {
    public const string Delete = "delete";
    public const string DontDelete = "dont-delete";
    private readonly IInteractionContext _context;
    private readonly IDiscordRestInteractionAPI _interactionApi;
    private readonly PromptService _prompt;

    public PromptDeletePrompt(IInteractionContext context, IDiscordRestInteractionAPI interactionAPI, PromptService prompt) {
        _context = context;
        _interactionApi = interactionAPI;
        _prompt = prompt;
    }

    [Button(Delete)]
    public async Task<IResult> OnDelete(string state) {
        var id = int.Parse(state);
        var getPrompt = await _prompt.GetPromptByIdAsync(id, CancellationToken);
        if(!getPrompt.IsDefined(out var prompt)) return getPrompt;

        var deletePrompt = await _prompt.DeletePromptAsync(prompt, CancellationToken);
        if(!deletePrompt.IsSuccess) return deletePrompt;

        return await _interactionApi.EditOriginalInteractionResponseAsync(
            _context.Interaction.ApplicationID,
            _context.Interaction.Token,
            embeds: new Embed[] { new(Description: $"Prompt #{state} has been deleted.") },
            components: Array.Empty<IMessageComponent>(),
            ct: CancellationToken
        );
    }

    [Button(DontDelete)]
    public async Task<IResult> OnDontDelete(string state) {
        return await _interactionApi.EditOriginalInteractionResponseAsync(
            _context.Interaction.ApplicationID,
            _context.Interaction.Token,
            embeds: new Embed[] { new(Description: $"Prompt #{state} lives. For now.") },
            components: Array.Empty<IMessageComponent>(),
            ct: CancellationToken
        );
    }
}
