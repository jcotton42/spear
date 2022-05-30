using Remora.Rest.Core;

namespace Spear.Models;

public class Prompt {
    public int Id { get; set; }
    public string Text { get; set; } = null!;
    public Snowflake? Submitter { get; set; }
    public Snowflake GuildId { get; set; }
}
