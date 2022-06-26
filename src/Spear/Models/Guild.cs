using Remora.Rest.Core;

namespace Spear.Models;

public class Guild {
    public Snowflake Id { get; set; }
    public string Name { get; set; } = null!;
    public Rating NsfwChannelRatingCap { get; set; }
    public Rating SafeChannelRatingCap { get; set; }

    public ICollection<Author> Authors { get; set; } = new HashSet<Author>();
    public ICollection<Book> Books { get; set; } = new HashSet<Book>();
    public ICollection<PermissionDefault> PermissionDefaults { get; set; } = new HashSet<PermissionDefault>();
    public ICollection<PermissionEntry> PermissionEntries { get; set; } = new HashSet<PermissionEntry>();
    public ICollection<Prompt> Prompts { get; set; } = new HashSet<Prompt>();
    public ICollection<Story> Stories { get; set; } = new HashSet<Story>();
}
