using Remora.Rest.Core;

namespace Spear.Models;

public class Book {
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public BookType Type { get; set; }
    public Snowflake? GuildId { get; set; }
}
