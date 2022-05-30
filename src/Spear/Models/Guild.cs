using Remora.Rest.Core;

namespace Spear.Models;

public class Guild {
    public Snowflake Id { get; set; }
    public List<Book> Books { get; set; } = null!;
    public List<PermissionDefault> PermissionDefaults { get; set; } = null!;
    public List<PermissionEntry> PermissionEntries { get; set; } = null!;
    public List<Prompt> Prompts { get; set; } = null!;
}
