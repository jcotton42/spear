using System.ComponentModel;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.Commands.Attributes;
using Remora.Discord.Commands.Conditions;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Feedback.Services;
using Remora.Results;
using Spear.Completers;
using Spear.Models;
using Spear.Services;

namespace Spear.Commands;

public partial class OldMan {
    [RequireContext(ChannelContext.Guild)]
    public class BookCommands : CommandGroup {
        private readonly BookService _books;
        private readonly ICommandContext _commandContext;
        private readonly FeedbackService _feedback;

        public BookCommands(BookService books, ICommandContext commandContext, FeedbackService feedback) {
            _books = books;
            _commandContext = commandContext;
            _feedback = feedback;
        }

        [Command("addbook")]
        public async Task<IResult> AddBookAsync(
            [Description("What kind of literature is it?")]
            BookType type,
            [Description("The book's rating")]
            Rating rating,
            [Description("The book you wish to add")] [Greedy]
            string title
        ) {
            var add = await _books.AddGuildBookAsync(title, type, rating, _commandContext.GuildID.Value, CancellationToken);
            if(add.IsSuccess) {
                return await _feedback.SendContextualSuccessAsync($"I have added {title} to my repertoire!",
                    ct: CancellationToken);
            }

            return add;
        }

        [Command("deletebook")]
        public async Task<IResult> DeleteBookAsync(
            [Description("What kind of literature is it?")]
            BookType type,
            [Description("The book you wish to remove from the bot. This has to be an exact match")]
            [Greedy]
            [AutocompleteProvider(BookTitleCompleter.Identity)]
            string title
        ) {
            var remove = await _books.RemoveBookFromGuildByTitleAsync(title, type, _commandContext.GuildID.Value, CancellationToken);
            if(remove.IsSuccess) {
                return await _feedback.SendContextualSuccessAsync($"I have removed {title} from my repertoire!",
                    ct: CancellationToken);
            }

            return remove;
        }
    }
}
