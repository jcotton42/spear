using Remora.Rest.Core;

namespace Spear.Models;

public class Guild {
    public Snowflake Id { get; set; }
    public string Name { get; set; } = null!;

    public List<Author> Authors { get; set; } = null!;
    public List<Book> Books { get; set; } = null!;
    public List<PermissionDefault> PermissionDefaults { get; set; } = null!;
    public List<PermissionEntry> PermissionEntries { get; set; } = null!;
    public List<Prompt> Prompts { get; set; } = null!;
    public List<Story> Stories { get; set; } = null!;
}
