using Remora.Rest.Core;

namespace Spear.Models;

public class Guild {
    public Snowflake Id { get; set; }
    public string Name { get; set; } = null!;

    public ICollection<Author> Authors { get; set; } = null!;
    public ICollection<Book> Books { get; set; } = null!;
    public ICollection<PermissionDefault> PermissionDefaults { get; set; } = null!;
    public ICollection<PermissionEntry> PermissionEntries { get; set; } = null!;
    public ICollection<Prompt> Prompts { get; set; } = null!;
    public ICollection<Story> Stories { get; set; } = null!;
}
