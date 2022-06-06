using Remora.Rest.Core;

namespace Spear.Models; 

public class PermissionDefault {
    public Snowflake GuildId { get; set; }
    public Permission Permission { get; set;}
    public PermissionMode Mode { get; set; }
}
