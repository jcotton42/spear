using System.ComponentModel;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.Commands.Conditions;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Feedback.Services;
using Remora.Results;
using Spear.Services;

namespace Spear.Commands;

public partial class OldMan {
    [RequireContext(ChannelContext.Guild)]
    public class MiscCommands : CommandGroup {
        private readonly BookService _books;
        private readonly ITextCommandContext _context;
        private readonly FeedbackService _feedback;

        public MiscCommands(BookService books, ITextCommandContext context, FeedbackService feedback) {
            _books = books;
            _context = context;
            _feedback = feedback;
        }

        [Command("fetch")]
        [Description("Tries to make fetch happen")]
        public async Task<IResult> FetchAsync() {
            var result = await _books.GetRandomGuildBook(_context.GuildID.Value, CancellationToken);
            var title = result.IsSuccess ? result.Entity : "Just Fourteen";

            return await _feedback.SendContextualNeutralAsync($"{title}! Stop trying to make fetch happen!",
                ct: CancellationToken);
        }
    }
}
