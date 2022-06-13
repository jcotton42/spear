using System.Collections.Concurrent;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Feedback.Messages;
using Remora.Discord.Commands.Feedback.Services;
using Remora.Discord.Interactivity;
using Remora.Results;

namespace Spear.Services;

public record Button(string Label, ButtonComponentStyle Style);

public class UserInputService {
    private const string CustomIdPrefix = "user-input";

    private static readonly ConcurrentDictionary<string, TaskCompletionSource<int>> Tasks = new();

    private readonly FeedbackService _feedback;

    public UserInputService(FeedbackService feedback) => _feedback = feedback;

    /// <summary>
    /// Requests input from the user in the current command context with buttons.
    /// </summary>
    /// <param name="promptText">The text to show for the input prompt.</param>
    /// <param name="buttons">The definitions for the buttons to show.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>The 0-based index of the button clicked.</returns>
    public async Task<Result<int>> RequestInputWithButtonsAsync(string promptText, IReadOnlyList<Button> buttons, CancellationToken ct) {
        var requestId = Guid.NewGuid().ToString("N");
        var embed = new Embed(Description: promptText);
        var options = new FeedbackMessageOptions(
            MessageFlags: MessageFlags.Ephemeral,
            MessageComponents: new IMessageComponent[] {
                new ActionRowComponent(buttons.Select((button, index) => new ButtonComponent(
                    Label: button.Label,
                    Style: button.Style,
                    CustomID: $"{CustomIdPrefix}:{requestId}:{index}"
                )).ToArray())
            }
        );

        var tcs = new TaskCompletionSource<int>();
        using var registration = ct.Register(() => tcs.SetCanceled(ct), useSynchronizationContext: false);
        if(!Tasks.TryAdd(requestId, tcs)) {
            // TODO somehow a GUID was reused
        }

        var prompt = await _feedback.SendContextualEmbedAsync(embed, options, ct);
        if(!prompt.IsSuccess) return Result<int>.FromError(prompt);

        return await tcs.Task;
    }

    public class UserInputResponderEntity : IButtonInteractiveEntity {
        public Task<Result<bool>> IsInterestedAsync(ComponentType? componentType, string customID, CancellationToken ct = new CancellationToken()) {
            return Task.FromResult<Result<bool>>(
                componentType == ComponentType.Button && customID.StartsWith(CustomIdPrefix)
            );
        }

        public Task<Result> HandleInteractionAsync(IUser user, string customID, CancellationToken ct = new CancellationToken()) {
            var split = customID.Split(':', 3);
            var requestId = split[1];
            var index = int.Parse(split[2]);

            if(Tasks.TryRemove(requestId, out var tcs)) {
                tcs.SetResult(index);
            } else {
                throw new InvalidOperationException($"User input request ID {requestId} did not exist");
            }

            return Task.FromResult(Result.FromSuccess());
        }
    }
}
