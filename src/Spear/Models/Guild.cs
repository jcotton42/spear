using Remora.Rest.Core;

namespace Spear.Models;

public class Guild {
    public Snowflake Id { get; set; }
    public List<Prompt> Prompts { get; set; } = null!;
}
