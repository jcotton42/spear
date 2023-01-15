using System.ComponentModel;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Conditions;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Feedback.Messages;
using Remora.Discord.Commands.Feedback.Services;
using Remora.Discord.Interactivity;
using Remora.Discord.Interactivity.Services;
using Remora.Discord.Pagination.Extensions;
using Remora.Results;
using Spear.Extensions;
using Spear.Services;

namespace Spear.Commands;

public partial class OldMan {
    [RequireContext(ChannelContext.Guild)]
    public class PromptCommands : CommandGroup {
        private readonly ICommandContext _commandContext;
        private readonly InMemoryDataService<string, TaskCompletionSource<string>> _data;
        private readonly FeedbackService _feedback;
        private readonly PromptService _prompt;

        public PromptCommands(ICommandContext commandContext, InMemoryDataService<string, TaskCompletionSource<string>> data,
            FeedbackService feedback, PromptService prompt) {
            _commandContext = commandContext;
            _data = data;
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

            var key = Guid.NewGuid().ToString("N");
            var tcs = new TaskCompletionSource<string>();
            _data.TryAddData(key, tcs);

            var embed = new Embed(Description: $"Are you sure you want to delete this prompt?\n>>> {prompt.Text}");
            var options = new FeedbackMessageOptions(
                MessageFlags: MessageFlags.Ephemeral,
                MessageComponents: new[] {
                    new ActionRowComponent(new[] {
                        new ButtonComponent(
                            Label: "Delete",
                            Style: ButtonComponentStyle.Danger,
                            CustomID: CustomIDHelpers.CreateButtonIDWithState(PromptDeletePrompt.Delete, key)
                        ),
                        new ButtonComponent(
                            Label: "Don't delete",
                            Style: ButtonComponentStyle.Secondary,
                            CustomID: CustomIDHelpers.CreateButtonIDWithState(PromptDeletePrompt.DontDelete, key)
                        ),
                    })
                }
            );

            using var registration = CancellationToken.Register(() => tcs.SetCanceled(CancellationToken), useSynchronizationContext: false);
            var confirmation = await _feedback.SendContextualEmbedAsync(embed, options, CancellationToken);
            if(!confirmation.IsSuccess) return Result.FromError(confirmation);

            var result = await tcs.Task;

            if(result == PromptDeletePrompt.DontDelete) {
                return await _feedback.SendContextualNeutralAsync("The prompt lives. For now.", ct: CancellationToken);
            }

            var delete = await _prompt.DeletePromptAsync(prompt, CancellationToken);
            if(!delete.IsSuccess) return delete;

            return await _feedback.SendContextualSuccessAsync(
                "I have deleted the prompt.",
                ct: CancellationToken
            );

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
    private readonly InMemoryDataService<string, TaskCompletionSource<string>> _data;

    public PromptDeletePrompt(InMemoryDataService<string, TaskCompletionSource<string>> data) => _data = data;

    [Button(Delete)]
    public async Task<Result> OnDelete(string key) {
        var getLease = await _data.LeaseDataAsync(key, CancellationToken);
        if(!getLease.IsDefined()) return Result.FromError(getLease);

        await using var lease = getLease.Entity;
        lease.Data.SetResult(Delete);
        lease.Delete();

        return Result.FromSuccess();
    }

    [Button(DontDelete)]
    public async Task<Result> OnDontDelete(string key) {
        var getLease = await _data.LeaseDataAsync(key, CancellationToken);
        if(!getLease.IsDefined()) return Result.FromError(getLease);

        await using var lease = getLease.Entity;
        lease.Data.SetResult(DontDelete);
        lease.Delete();

        return Result.FromSuccess();
    }
}
