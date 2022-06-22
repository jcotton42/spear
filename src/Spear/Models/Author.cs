using Remora.Rest.Core;

namespace Spear.Models;

public class Author {
    public int Id { get; set; }
    public Snowflake GuildId { get; set; }
    public string Name { get; set; } = null!;

    public ICollection<AuthorProfile> Profiles { get; set; } = new HashSet<AuthorProfile>();
    public ICollection<Story> Stories { get; set; } = new HashSet<Story>();
}
