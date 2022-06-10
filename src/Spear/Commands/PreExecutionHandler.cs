using Remora.Discord.Commands.Contexts;
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
            InteractionContext ic => ic.Data.Name.Value.Equals("oldman", StringComparison.OrdinalIgnoreCase),
            MessageContext mc => mc.Message.Content.Value[mc.Message.Content.Value.IndexOf(' ')..].TrimStart()
                .StartsWith("oldman", StringComparison.OrdinalIgnoreCase),
            _ => false,
        };
        if(!calledMeOld) {
            return Result.FromSuccess();
        }

        var getBook = await _book.GetRandomGuildBook(context.GuildID.Value, ct);
        var book = getBook.IsSuccess ? getBook.Entity : "Just Fourteen";

        var reply = await _feedback.SendContextualNeutralAsync($"{book}! Who are you calling _OLD_ man?!", ct: ct);
        return reply.IsSuccess ? Result.FromSuccess() : Result.FromError(reply);
    }
}
