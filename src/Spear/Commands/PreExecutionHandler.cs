using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Extensions;
using Remora.Discord.Commands.Feedback.Services;
using Remora.Discord.Commands.Services;
using Remora.Results;
using Spear.Services;

namespace Spear.Commands;

public class PreExecutionHandler : IPreExecutionEvent {
    private readonly BookService _book;
    private readonly FeedbackService _feedback;

    public PreExecutionHandler(BookService book, FeedbackService feedback) {
        _book = book;
        _feedback = feedback;
    }

    public async Task<Result> BeforeExecutionAsync(ICommandContext context, CancellationToken ct) {
        var calledMeOld = context switch {
            InteractionContext ic =>
                ic.Interaction.Data.Value.TryPickT0(out var acd, out _)
                && acd.Name.Equals("oldman", StringComparison.OrdinalIgnoreCase),
            MessageContext mc => mc.Message.Content.Value[mc.Message.Content.Value.IndexOf(' ')..].TrimStart()
                .StartsWith("oldman", StringComparison.OrdinalIgnoreCase),
            _ => false,
        };
        if(!calledMeOld) {
            return Result.FromSuccess();
        }

        string book;
        if(context.TryGetGuildID(out var guildId)) {
            var getBook = await _book.GetRandomGuildBook(guildId.Value, ct);
            book = getBook.IsSuccess ? getBook.Entity : "Just Fourteen";
        } else {
            book = "Just Fourteen";
        }

        var reply = await _feedback.SendContextualNeutralAsync($"{book}! Who are you calling _OLD_ man?!", ct: ct);
        return reply.IsSuccess ? Result.FromSuccess() : Result.FromError(reply);
    }
}
