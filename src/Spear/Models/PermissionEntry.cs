using Remora.Rest.Core;

namespace Spear.Models; 

public class PermissionEntry {
    public Snowflake GuildId { get; set; }
    public Snowflake RoleId { get; set; }
    public Permission Permission { get; set; }
    public PermissionMode Mode { get; set; }
}
