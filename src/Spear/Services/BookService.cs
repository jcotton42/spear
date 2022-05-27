using EntityFramework.Exceptions.Common;
using Microsoft.EntityFrameworkCore;
using Remora.Rest.Core;
using Remora.Results;
using Spear.Models;

namespace Spear.Services;

public class BookService {
    private readonly SpearContext _context;

    public BookService(SpearContext context) {
        _context = context;
    }

    /// <summary>
    /// Adds a book for a specific guild.
    /// </summary>
    /// <param name="title">The book's title.</param>
    /// <param name="type">The book's type.</param>
    /// <param name="guild">The snowflake of the guild to add the book for.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>
    /// <para>
    /// A successful <see cref="Result{TEntity}"/> with the book's ID if the book was not already present for given guild.
    /// </para>
    /// <para>
    /// A <see cref="Result{TEntity}"/> with an error of <see cref="InvalidOperationError"/> if the book already
    /// existed for that guild.
    /// </para>
    /// </returns>
    public Task<Result<int>> AddGuildBookAsync(string title, BookType type, Snowflake guild, CancellationToken ct) => AddBookAsync(title, type, guild, ct);

    /// <summary>
    /// Adds a book available across all guilds.
    /// </summary>
    /// <param name="title">The book's title.</param>
    /// <param name="type">The book's type.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>
    /// <para>
    /// A successful <see cref="Result{TEntity}"/> with the book's ID if the book was not already present.
    /// </para>
    /// <para>
    /// A <see cref="Result{TEntity}"/> with an error of <see cref="InvalidOperationError"/> if the book already existed.
    /// </para>
    /// </returns>
    public Task<Result<int>> AddGlobalBookAsync(string title, BookType type, CancellationToken ct) => AddBookAsync(title, type, null, ct);

    private async Task<Result<int>> AddBookAsync(string title, BookType type, Snowflake? guild, CancellationToken ct) {
        var book = new Book { Title = title, Type = type, GuildId = guild };
        _context.Books.Add(book);
        try {
            await _context.SaveChangesAsync(ct);
            return book.Id;
        } catch(UniqueConstraintException) {
            return new InvalidOperationError(
                $"'{title}' with type {type} is already registered for guild {guild}"
            );
        }
    }

    /// <summary>
    /// Gets a random book of the given type for the given guild.
    /// </summary>
    /// <param name="type">The type of book.</param>
    /// <param name="guild">The guild to search in.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>
    /// <para>
    /// A successful <see cref="Result{TEntity}"/> with the book's title if the book was found.
    /// </para>
    /// <para>
    /// A <see cref="Result{TEntity}"/> with an error of <see cref="NotFoundError"/> if no matching book could be found.
    /// </para>
    /// </returns>
    public async Task<Result<string>> GetRandomGuildBookOfType(BookType type, Snowflake guild, CancellationToken ct) {
        // TODO cache this
        var ids = await _context.Books
            .Where(b => b.GuildId == guild && b.Type == type)
            .Select(b => b.Id)
            .ToListAsync(ct);

        if(ids.Any()) {
            var id = ids[Random.Shared.Next(ids.Count)];
            var book = await _context.Books
                .SingleAsync(b => b.Id == id, ct);
            return book.Title;
        } else {
            return new NotFoundError(
                $"No book of type {type} was found for guild {guild}."
            );
        }
    }

    /// <summary>
    /// Removes books for a specific guild by their title and type.
    /// </summary>
    /// <param name="title">The book's title.</param>
    /// <param name="type">The book's type.</param>
    /// <param name="guild">The guild to search in.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>
    /// <para>
    /// A successful <see cref="Result{TEntity}"/> with the number of books deleted if matches were found.
    /// </para>
    /// <para>
    /// A <see cref="Result{TEntity}"/> with an error of <see cref="NotFoundError"/> if no matching book could be found.
    /// </para>
    /// </returns>
    public async Task<Result<int>> RemoveBookFromGuildByTitleAsync(string title, BookType type, Snowflake guild, CancellationToken ct) {
        var books = await _context.Books
            .Where(b => b.Title == title && b.Type == type && b.GuildId == guild)
            .ToListAsync(ct);
        _context.Books.RemoveRange(books);
        var affected = await _context.SaveChangesAsync(ct);

        if(affected > 0) {
            return affected;
        } else {
            return new NotFoundError(
                $"No books matching title {title} and {type} were found for removal."
            );
        }
    }
}
