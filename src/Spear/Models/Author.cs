using Remora.Rest.Core;

namespace Spear.Models;

public class Author {
    public int Id { get; set; }
    public Snowflake GuildId { get; set; }

    public ICollection<AuthorProfile> Profiles { get; set; } = null!;
    public ICollection<Story> Stories { get; set; } = null!;
}
