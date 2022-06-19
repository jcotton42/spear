using System.ComponentModel;
using Microsoft.EntityFrameworkCore;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.Commands.Conditions;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Feedback.Services;
using Remora.Results;
using Spear.Models;

namespace Spear.Commands;

public partial class OldMan {
    [RequireContext(ChannelContext.Guild)]
    public class StoryCommands : CommandGroup {
        private readonly ICommandContext _commandContext;
        private readonly FeedbackService _feedback;
        private readonly SpearContext _spearContext;

        public StoryCommands(ICommandContext commandContext, FeedbackService feedback, SpearContext spearContext) {
            _commandContext = commandContext;
            _feedback = feedback;
            _spearContext = spearContext;
        }

        [Command("recommend")]
        [Description("Recommends a fic")]
        public async Task<IResult> RecommendAsync(
            [Description("The fic's title")]
            string title,
            [Description("The fic's author")]
            string author,
            [Description("The fic's rating")]
            Rating rating,
            [Description("The story's completion status")]
            StoryStatus status,
            [Description("A semicolon-separated list of fandoms this fic is in")]
            string fandoms,
            [Description("A semicolon-separated list of URLs the fic can be found at")]
            string urls,
            [Description("A semicolon-separated list of ships")]
            string? ships = null,
            [Description("A semicolon-separated list of tags")]
            string? tags = null,
            [Description("A summary for the fic")]
            [Greedy]
            string? summary = null
        ) {
            // TODO check length
            var fandomList = fandoms.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToArray();
            // TODO check length
            var urlList = urls.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(u => new Uri(u)).ToArray();
            // TODO check length
            var shipList = ships?.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                ?.ToArray() ?? Array.Empty<string>();
            // TODO check length
            var tagList = tags?.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                ?.ToArray() ?? Array.Empty<string>();

            var authorModel = new Author {GuildId = _commandContext.GuildID.Value};
            _spearContext.Authors.Add(authorModel);
            await _spearContext.SaveChangesAsync(CancellationToken);

            var existingTags = await _spearContext.Tags.Where(tag =>
                (tag.Type == TagType.Fandom && fandomList.Contains(tag.Name))
                || (tag.Type == TagType.Ship && shipList.Contains(tag.Name))
                || (tag.Type == TagType.General && tagList.Contains(tag.Name))
            ).ToListAsync(CancellationToken);

            var tagsToSave = new HashSet<Tag>(existingTags);
            foreach(var fandom in fandomList) tagsToSave.Add(new Tag {Name = fandom, Type = TagType.Fandom});
            foreach(var ship in shipList) tagsToSave.Add(new Tag {Name = ship, Type = TagType.Ship});
            foreach(var tag in tagList) tagsToSave.Add(new Tag {Name = tag, Type = TagType.General});

            var story = new Story {
                AuthorId = authorModel.Id,
                GuildId = _commandContext.GuildID.Value,
                Rating = rating,
                Status = status,
                Summary = summary,
                Title = title,
                Reactions =
                    new HashSet<StoryReaction> {new() {UserId = _commandContext.User.ID, Reaction = Reaction.Like}},
                Urls = urlList.Select(u => new StoryUrl {Url = u}).ToList(),
                Tags = tagsToSave,
            };
            _spearContext.Stories.Add(story);
            await _spearContext.SaveChangesAsync(CancellationToken);

            return await _feedback.SendContextualNeutralAsync($"{title} added!", ct: CancellationToken);
        }
    }
}
