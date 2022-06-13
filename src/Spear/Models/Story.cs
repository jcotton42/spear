using Remora.Rest.Core;

namespace Spear.Models;

public class Story {
    public int Id { get; set; }
    public Snowflake GuildId { get; set; }
    public string Title { get; set; } = null!;
    public int AuthorId { get; set; }
    public string? Summary { get; set; }
    public Rating Rating { get; set; }
    public StoryStatus Status { get; set; }

    public List<StoryReaction> Reactions { get; set; } = null!;
    public List<Tag> Tags { get; set; } = null!;
    public List<StoryUrl> Urls { get; set; } = null!;
}
