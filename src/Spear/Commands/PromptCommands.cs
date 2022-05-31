using System.ComponentModel;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.Commands.Conditions;
using Remora.Discord.Commands.Feedback.Services;
using Remora.Results;
using Spear.Conditions.Attributes;
using Spear.Services;

namespace Spear.Commands;

public partial class OldMan {
    [RequireContext(ChannelContext.Guild)]
    [RequireRegisteredGuild]
    public class PromptCommands : CommandGroup {
        private readonly FeedbackService _feedback;
        private readonly PromptService _prompt;

        public PromptCommands(FeedbackService feedback, PromptService prompt) {
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
            var delete = await _prompt.DeletePromptAsync(id, CancellationToken);
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
                $"May I suggest the following{creditLine}?\n#`{prompt.Id}`\n >>> {prompt.Text}",
                ct: CancellationToken
            );
        }
    }
}
