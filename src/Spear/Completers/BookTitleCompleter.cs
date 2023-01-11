using FuzzySharp;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Autocomplete;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Extensions;
using Spear.Models;
using Spear.Services;

namespace Spear.Completers;

public class BookTitleCompleter : IAutocompleteProvider {
    public const string Identity = "autocomplete::book_titles";

    private readonly BookService _books;
    private readonly ICommandContext _commandContext;

    string IAutocompleteProvider.Identity => Identity;

    public BookTitleCompleter(BookService books, ICommandContext commandContext) {
        _books = books;
        _commandContext = commandContext;
    }

    public async ValueTask<IReadOnlyList<IApplicationCommandOptionChoice>> GetSuggestionsAsync(
        IReadOnlyList<IApplicationCommandInteractionDataOption> options,
        string userInput,
        CancellationToken ct
    ) {
        if(!_commandContext.TryGetGuildID(out var guildId)) return Array.Empty<IApplicationCommandOptionChoice>();

        var typeOption = options.First(o => o.Name == "type");
        if(!typeOption.Value.IsDefined(out var typeString) || !Enum.TryParse<BookType>(typeString.AsT0, out var type)) {
            return Array.Empty<IApplicationCommandOptionChoice>();
        }

        var books = await _books.GetAllGuildBooksOfTypeAsync(guildId.Value, type, ct);

        return books
            .OrderByDescending(book => Fuzz.Ratio(userInput, book))
            .Take(25)
            .Select(book => new ApplicationCommandOptionChoice(book, book))
            .ToList();
    }
}
