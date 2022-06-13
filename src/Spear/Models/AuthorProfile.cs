namespace Spear.Models;

public class AuthorProfile {
    public int Id { get; set; }
    public int AuthorId { get; set; }
    public string Pseud { get; set; } = null!;
    public Uri Url { get; set; } = null!;
    public bool UrlIsCanonical { get; set; }
}
