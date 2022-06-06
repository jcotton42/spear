using System.ComponentModel;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Commands.Conditions;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Feedback.Services;
using Remora.Results;
using Spear.Services;

namespace Spear.Commands;

public partial class OldMan {
    [RequireContext(ChannelContext.Guild)]
    [RequireDiscordPermission(DiscordPermission.ManageGuild)]
    public class GuildCommands : CommandGroup {
        private readonly ICommandContext _commandContext;
        private readonly FeedbackService _feedback;
        private readonly GuildService _guild;
        private readonly IDiscordRestGuildAPI _guildApi;

        public GuildCommands(ICommandContext commandContext, FeedbackService feedback, GuildService guild, IDiscordRestGuildAPI guildApi) {
            _commandContext = commandContext;
            _feedback = feedback;
            _guild = guild;
            _guildApi = guildApi;
        }

        [Command("register")]
        [Description("Registers this server with the bot. This is usually done for you automatically upon join.")]
        public async Task<IResult> RegisterAsync() {
            var getGuild = await _guildApi.GetGuildAsync(_commandContext.GuildID.Value, ct: CancellationToken);
            if(!getGuild.IsDefined(out var guild)) return getGuild;

            await _guild.UpsertGuildAsync(_commandContext.GuildID.Value, guild.Name, CancellationToken);

            return await _feedback.SendContextualSuccessAsync("I've registered your server!", ct: CancellationToken);
        }
    }
}
