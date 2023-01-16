using Remora.Commands.Results;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Extensions;
using Remora.Discord.Commands.Feedback.Services;
using Remora.Discord.Commands.Services;
using Remora.Results;
using Spear.Extensions;
using Spear.Services;

namespace Spear.Commands;

public class PostExecutionHandler : IPostExecutionEvent {
    private readonly BookService _books;
    private readonly FeedbackService _feedback;
    private readonly ILogger _logger;

    public PostExecutionHandler(BookService books, FeedbackService feedback, ILogger<PostExecutionHandler> logger) {
        _books = books;
        _feedback = feedback;
        _logger = logger;
    }

    public async Task<Result> AfterExecutionAsync(ICommandContext context, IResult commandResult, CancellationToken ct = default) {
        if(commandResult.IsSuccess) return Result.FromSuccess();

        var error = commandResult.Error is ConditionNotSatisfiedError
            ? commandResult.GetFirstInnerErrorOfNotType<ConditionNotSatisfiedError>()!
            : commandResult.Error!;
        string message;
        if(error is ExceptionError ee) {
            message = "Something went wrong. Check the logs!";
            _logger.LogError(ee.Exception, ee.Exception.Message);
        } else {
            message = error.Message;
        }

        string title;
        if(context.TryGetGuildID(out var guildId)) {
            var book = await _books.GetRandomGuildBook(guildId.Value, ct);
            title = book.IsSuccess ? book.Entity : "Just Fourteen";
        } else {
            title = "Just Fourteen";
        }

        var reply = await _feedback.SendContextualErrorAsync($"{title}! {message}", ct: ct);
        if(reply.IsSuccess) {
            return Result.FromSuccess();
        }

        _logger.LogError("Reply failed: {Error}", reply.Error);
        return Result.FromError(reply);
    }
}
