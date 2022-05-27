using Remora.Commands.Results;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Feedback.Services;
using Remora.Discord.Commands.Services;
using Remora.Results;
using Spear.Extensions;
using Spear.Services;

namespace Spear.Commands;

public class PostExecutionHandler : IPostExecutionEvent {
    private readonly BookService _books;
    private readonly FeedbackService _feedback;

    public PostExecutionHandler(BookService books, FeedbackService feedback) {
        _books = books;
        _feedback = feedback;
    }

    public async Task<Result> AfterExecutionAsync(ICommandContext context, IResult commandResult, CancellationToken ct = default) {
        if(commandResult.IsSuccess) return Result.FromSuccess();

        var error = commandResult.Error is ConditionNotSatisfiedError
            ? commandResult.GetFirstInnerErrorOfNotType<ConditionNotSatisfiedError>()!
            : commandResult.Error!;
        var book = await _books.GetRandomGuildBook(context.GuildID.Value, ct);
        var title = book.IsSuccess ? book.Entity : "Just Fourteen";

        var reply = await _feedback.SendContextualErrorAsync($"{title}! {error.Message}", ct: ct);
        return reply.IsSuccess ? Result.FromSuccess() : Result.FromError(reply);
    }
}
