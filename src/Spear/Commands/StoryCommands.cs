using System.ComponentModel;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.Commands.Conditions;
using Remora.Discord.Commands.Feedback.Services;
using Remora.Discord.Extensions.Embeds;
using Remora.Results;
using Spear.Models;
using Spear.Services;

namespace Spear.Commands;

public partial class OldMan {
    [Group("story")]
    [RequireContext(ChannelContext.Guild)]
    public class StoryCommands : CommandGroup {
        private readonly FeedbackService _feedback;
        private readonly StoryService _stories;

        public StoryCommands(FeedbackService feedback, StoryService stories) {
            _feedback = feedback;
            _stories = stories;
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
                .ToArray();
            // TODO check length
            var shipList = ships?.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                ?.ToArray() ?? Array.Empty<string>();
            // TODO check length
            var tagList = tags?.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                ?.ToArray() ?? Array.Empty<string>();

            var addStory = await _stories.RecommendStoryAsync(title, author, rating, status, fandomList, urlList,
                shipList, tagList, summary, CancellationToken);
            if(!addStory.IsDefined(out var story)) return addStory;

            return await _feedback.SendContextualNeutralAsync($"{title} #`{story.Id}` added!", ct: CancellationToken);
        }

        [Command("random")]
        [Description("Pulls a random story, excluding ones you've already responded to")]
        public async Task<IResult> RandomAsync() {
            var get = await _stories.GetRandomStoryAsync(CancellationToken);
            if(!get.IsDefined(out var story)) return get;

            var embed = new EmbedBuilder()
                .WithAuthor(story.Author)
                .WithTitle($"{story.Title} (#`{story.Id}`)")
                .AddField("Rating", story.Rating.ToString(), inline: true).Entity
                .AddField("Status", story.Status.ToString(), inline: true).Entity
                .AddField("Fandoms", string.Join(", ", story.Fandoms.OrderBy(f => f)), inline: true).Entity
                .AddField("Ships", string.Join(", ", story.Ships.OrderBy(s => s).DefaultIfEmpty("None")), inline: true).Entity
                .AddField("Tags", string.Join(", ", story.Tags.OrderBy(t => t).DefaultIfEmpty("None")), inline: true).Entity
                .AddField("\u200B", "\u200B", inline: true).Entity
                .AddField("URLs", string.Join("\n", story.Urls.OrderBy(u => u))).Entity
                .AddField("Summary", story.Summary ?? "None").Entity
                .WithFooter($"{story.LikeCount} 👍/{story.IndifferentCount} 🤷/{story.DislikeCount} 👎");

            return await _feedback.SendContextualEmbedAsync(embed.Build().Entity, ct: CancellationToken);
        }
    }
}
