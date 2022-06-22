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

    public ICollection<StoryReaction> Reactions { get; set; } = new HashSet<StoryReaction>();
    public ICollection<Tag> Tags { get; set; } = new HashSet<Tag>();
    public ICollection<StoryUrl> Urls { get; set; } = new HashSet<StoryUrl>();
}
