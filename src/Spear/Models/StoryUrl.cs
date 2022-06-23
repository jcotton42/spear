namespace Spear.Models;

public class StoryUrl {
    public int Id { get; set; }
    public int StoryId { get; set; }
    public Uri Url { get; set; } = null!;
    public bool IsNormalized { get; set; }
}
