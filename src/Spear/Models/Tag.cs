namespace Spear.Models;

public class Tag : IEquatable<Tag> {
    public int Id { get; set; }
    public string Name { get; init; } = null!;
    public TagType Type { get; init; }

    public ICollection<Story> Stories { get; set; } = new HashSet<Story>();

    public bool Equals(Tag? other) {
        if (ReferenceEquals(null, other)) {
            return false;
        }

        if (ReferenceEquals(this, other)) {
            return true;
        }

        return Name == other.Name && Type == other.Type;
    }

    public override bool Equals(object? obj) => obj is Tag t && t.Equals(this);

    public override int GetHashCode() => HashCode.Combine(Name, Type);

    public static bool operator ==(Tag? left, Tag? right) => Equals(left, right);

    public static bool operator !=(Tag? left, Tag? right) => !Equals(left, right);
}
