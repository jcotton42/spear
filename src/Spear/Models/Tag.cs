namespace Spear.Models;

public class Tag {
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public TagType Type { get; set; }

    public List<Story> Stories { get; set; } = null!;
}
