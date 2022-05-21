using System.Reflection.Metadata.Ecma335;
using Remora.Commands.Extensions;
using Remora.Discord.API;
using Remora.Discord.API.Abstractions.Gateway.Commands;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Extensions;
using Remora.Discord.Commands.Services;
using Remora.Discord.Gateway;
using Remora.Discord.Hosting.Extensions;
using Remora.Rest.Core;
using Spear.Commands;

var host = Host
    .CreateDefaultBuilder(args)
    .AddDiscordService(services => {
        var config = services.GetRequiredService<IConfiguration>();

        return config.GetValue<string?>("DiscordToken") ?? throw new InvalidOperationException(
            "No bot token provided"
        );
    })
    .ConfigureServices((context, services) => {
        services.Configure<DiscordGatewayClientOptions>(options => options.Intents = GatewayIntents.Guilds);

        services
            .AddDiscordCommands(enableSlash: true)
            .AddCommandTree()
                .WithCommandGroup<PromptCommands>();
    })
    .Build();

Snowflake? testGuild = null;
var env = host.Services.GetRequiredService<IHostEnvironment>();
var config = host.Services.GetRequiredService<IConfiguration>();
var logger = host.Services.GetRequiredService<ILogger<Program>>();

if(!env.IsProduction()) {
    if(config.GetValue<string?>("TestGuild") is string testGuildString) {
        if(!DiscordSnowflake.TryParse(testGuildString, out testGuild)) {
            logger.LogCritical("Could not parse test guild from environment.");
            return 1;
        }
    } else {
        logger.LogCritical("Could not find test guild in configuration");
        return 1;
    }
}

var slashService = host.Services.GetRequiredService<SlashService>();
var slashSupport = slashService.SupportsSlashCommands();
if(slashSupport.IsSuccess) {
    var updateSlash = await slashService.UpdateSlashCommandsAsync(testGuild);
    if(!updateSlash.IsSuccess) {
        logger.LogCritical("Failed to update slash commands: {Reason}", updateSlash.Error?.Message);
        return 1;
    }
} else {
    logger.LogCritical("The registered commands of the bot don't support slash commands: {Reason}", slashSupport.Error?.Message);
}

await host.RunAsync();

return 0;
