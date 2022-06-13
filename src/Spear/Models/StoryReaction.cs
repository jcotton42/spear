using Remora.Rest.Core;

namespace Spear.Models;

public class StoryReaction {
    public int StoryId { get; set; }
    public Snowflake UserId { get; set; }
    public Reaction Reaction { get; set; }
}
