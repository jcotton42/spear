using Remora.Rest.Core;

namespace Spear.Models;

public class Author {
    public int Id { get; set; }
    public Snowflake GuildId { get; set; }

    public List<AuthorProfile> Profiles { get; set; } = null!;
    public List<Story> Stories { get; set; } = null!;
}
