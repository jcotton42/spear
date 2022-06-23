using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Remora.Discord.Commands.Contexts;
using Remora.Results;
using Spear.Models;
using Spear.Results;

namespace Spear.Services;

public class StoryService {
    private readonly AuthorizationService _authorization;
    private readonly ICommandContext _commandContext;
    private readonly SpearContext _spearContext;

    private static readonly (string SiteName, string DomainPattern, string UrlPattern, string ReplacementPattern)[] SitePatterns = {
        (
            "fanfiction.net",
            @"^(www\.|m\.)?fanfiction\.net$",
            @"(?in)^(https?://)?((www|m)\.)?fanfiction\.net/s/(?<id>\d+).*",
            @"https://www.fanfiction.net/s/${id}"),
        (
            "archiveofourown.org",
            @"^(www\.)?archiveofourown\.org$",
            @"(?in)^(https?://)?(www\.)?archiveofourown\.org/works/(?<id>\d+).*",
            @"https://archiveofourown.org/works/${id}"),
        (
            "fimfiction.net",
            @"^(www\.)?fimfiction\.net$",
            @"(?in)^(https?://)?(www\.)?fimfiction\.net/story/(?<id>\d+).*",
            @"https://www.fimfiction.net/story/${id}"),
    };

    public StoryService(AuthorizationService authorization, ICommandContext commandContext, SpearContext spearContext) {
        _authorization = authorization;
        _commandContext = commandContext;
        _spearContext = spearContext;
    }

    public async Task<Result<Story>> RecommendStoryAsync(string title, string author, Rating rating, StoryStatus status,
        string[] fandoms, string[] urls, string[] ships, string[] tags, string? summary, CancellationToken ct) {
        var normalizedUrls = new List<(Uri Url, bool Normalized)>();
        foreach(var url in urls) {
            var normalize = NormalizeUrl(url);
            if(!normalize.IsDefined(out var normalizedUrl)) return Result<Story>.FromError(normalize);
            normalizedUrls.Add(normalizedUrl);
        }

        if(!_commandContext.GuildID.IsDefined(out var guildId)) {
            return new InvalidOperationError("Cannot be used outside a guild");
        }

        var queryCanSubmit = await _authorization.InvokerCanSubmitStoriesAsync(ct);
        if(!queryCanSubmit.IsDefined(out var canSubmit)) return Result<Story>.FromError(queryCanSubmit);
        if(!canSubmit) {
            return Result<Story>.FromError(new SpearPermissionDeniedError("You can't submit stories",
                Permission.SubmitStories));
        }

        // TODO check for existing story that matches?
        var authorModel = await _spearContext.Authors
            .FirstOrDefaultAsync(a => a.GuildId == guildId && (a.Name == author || a.Profiles.Any(ap => ap.Pseud == author)), ct);
        if(authorModel is null) {
            authorModel = new Author {GuildId = guildId, Name = author};
            _spearContext.Authors.Add(authorModel);
        }

        var existingTags = await _spearContext.Tags.Where(tag =>
            (tag.Type == TagType.Fandom && fandoms.Contains(tag.Name))
            || (tag.Type == TagType.Ship && ships.Contains(tag.Name))
            || (tag.Type == TagType.General && tags.Contains(tag.Name))
        ).ToListAsync(ct);

        var tagsToSave = new HashSet<Tag>(existingTags);
        foreach(var fandom in fandoms) tagsToSave.Add(new Tag {Name = fandom, Type = TagType.Fandom});
        foreach(var ship in ships) tagsToSave.Add(new Tag {Name = ship, Type = TagType.Ship});
        foreach(var tag in tags) tagsToSave.Add(new Tag {Name = tag, Type = TagType.General});

        var story = new Story {
            GuildId = guildId,
            Rating = rating,
            Status = status,
            Summary = summary,
            Title = title,
            Reactions = new HashSet<StoryReaction> {new() {UserId = _commandContext.User.ID, Reaction = Reaction.Like}},
            Urls = normalizedUrls.Select(u => new StoryUrl {IsNormalized = u.Normalized, Url = u.Url}).ToHashSet(),
            Tags = tagsToSave
        };
        _spearContext.Stories.Add(story);
        authorModel.Stories.Add(story);

        await _spearContext.SaveChangesAsync(ct);

        return story;
    }

    private static Result<(Uri, bool)> NormalizeUrl(string url) {
        if(!Uri.TryCreate(url, UriKind.Absolute, out var uri)) {
            if(!Uri.TryCreate($"https://{url}", UriKind.Absolute, out uri)) {
                return new ArgumentInvalidError(nameof(url), "Could not parse URL");
            }
        }

        foreach(var (siteName, domainPattern, urlPattern, replacementPattern) in SitePatterns) {
            var domainMatch = Regex.Match(uri.Host, domainPattern, RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
            if(!domainMatch.Success) continue;

            var urlMatch = Regex.Match(uri.AbsoluteUri, urlPattern, RegexOptions.CultureInvariant);
            if(!urlMatch.Success) {
                return new ArgumentInvalidError("url", $"{uri} appears to be a {siteName} URL but is malformed");
            }

            return (new Uri(urlMatch.Result(replacementPattern)), true);
        }

        return (uri, false);
    }
}
