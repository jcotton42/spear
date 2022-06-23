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

    public StoryService(AuthorizationService authorization, ICommandContext commandContext, SpearContext spearContext) {
        _authorization = authorization;
        _commandContext = commandContext;
        _spearContext = spearContext;
    }

    public async Task<Result<Story>> RecommendStoryAsync(string title, string author, Rating rating, StoryStatus status,
        string[] fandoms, Uri[] urls, string[] ships, string[] tags, string? summary, CancellationToken ct) {
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
            Urls = urls.Select(u => new StoryUrl {Url = u}).ToHashSet(),
            Tags = tagsToSave
        };
        _spearContext.Stories.Add(story);
        authorModel.Stories.Add(story);

        await _spearContext.SaveChangesAsync(ct);

        return story;
    }
}
