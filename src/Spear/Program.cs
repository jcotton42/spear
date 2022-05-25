using Microsoft.EntityFrameworkCore;
using Remora.Commands.Extensions;
using Remora.Discord.API;
using Remora.Discord.API.Abstractions.Gateway.Commands;
using Remora.Discord.Commands.Extensions;
using Remora.Discord.Commands.Responders;
using Remora.Discord.Commands.Services;
using Remora.Discord.Gateway;
using Remora.Discord.Hosting.Extensions;
using Remora.Rest.Core;
using Spear.Commands;
using Spear.Models;
using Spear.Services;

var host = Host
    .CreateDefaultBuilder(args)
    .AddDiscordService(services => {
        var config = services.GetRequiredService<IConfiguration>();

        return config.GetValue<string?>("DiscordToken") ?? throw new InvalidOperationException(
            "No bot token provided"
        );
    })
    .ConfigureServices((hostContext, services) => {
        services.Configure<DiscordGatewayClientOptions>(options => options.Intents = GatewayIntents.Guilds);
        services.Configure<InteractionResponderOptions>(options => options.SuppressAutomaticResponses = true);
        var host = hostContext.Configuration["PgHost"];
        var database = hostContext.Configuration["PgDatabase"];
        var username = hostContext.Configuration["PgUsername"];
        var password = hostContext.Configuration["PgPassword"];
        services.AddDbContext<SpearContext>(options => options.UseNpgsql(
            $"Host={host};Database={database};Username={username};Password={password}"
        ));

        services.AddScoped<BookService>();

        services
            .AddDiscordCommands(enableSlash: true)
            .AddCommandTree()
                .WithCommandGroup<GuildCommands>()
                .WithCommandGroup<PromptCommands>()
                .WithCommandGroup<MiscCommands>();
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
