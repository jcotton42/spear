using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql;
using Remora.Commands.Extensions;
using Remora.Discord.API;
using Remora.Discord.API.Abstractions.Gateway.Commands;
using Remora.Discord.Commands.Extensions;
using Remora.Discord.Commands.Responders;
using Remora.Discord.Commands.Services;
using Remora.Discord.Gateway;
using Remora.Discord.Gateway.Extensions;
using Remora.Discord.Hosting.Extensions;
using Remora.Discord.Interactivity.Extensions;
using Remora.Discord.Pagination.Extensions;
using Remora.Rest.Core;
using Spear.Commands;
using Spear.Completers;
using Spear.Models;
using Spear.Responders;
using Spear.Services;

var host = Host
    .CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(config => {
        config.AddKeyPerFile("/run/secrets", optional: true);
    })
    .AddDiscordService(services => {
        var config = services.GetRequiredService<IConfiguration>();

        return config.GetValue<string?>("DiscordToken") ?? throw new InvalidOperationException(
            "No bot token provided"
        );
    })
    .ConfigureServices((hostContext, services) => {
        var appId = hostContext.Configuration["DiscordAppId"];
        if(appId is not null) {
            services.Configure<CommandResponderOptions>(options => options.Prefix = $"<@{appId}>");
        }

        services.Configure<DiscordGatewayClientOptions>(options =>
            options.Intents = GatewayIntents.Guilds | GatewayIntents.GuildMessages);
        services.Configure<InteractionResponderOptions>(options => options.SuppressAutomaticResponses = true);
        var host = hostContext.Configuration["PgHost"];
        var database = hostContext.Configuration["PgDatabase"];
        var username = hostContext.Configuration["PgUsername"];
        var password = hostContext.Configuration["PgPassword"];
        services.AddDbContext<SpearContext>(options => options.UseNpgsql(
            $"Host={host};Database={database};Username={username};Password={password}"
        ));

        services
            .AddLazyCache()
            .AddScoped<AuthorizationService>()
            .AddScoped<BookService>()
            .AddScoped<GuildService>()
            .AddScoped<PromptService>();

        services
            .AddResponder<RegistrationResponder>()
            .AddDiscordCommands(enableSlash: true)
            .AddPreExecutionEvent<PreExecutionHandler>()
            .AddPostExecutionEvent<PostExecutionHandler>()
            .AddCommandTree()
                .WithCommandGroup<OldMan>()
                .WithCommandGroup<OldMan.AuthorizationCommands>()
                .WithCommandGroup<OldMan.BookCommands>()
                .WithCommandGroup<OldMan.GuildCommands>()
                .WithCommandGroup<OldMan.MiscCommands>()
                .WithCommandGroup<OldMan.PromptCommands>()
            .Finish()
            .AddAutocompleteProvider<BookTitleCompleter>()
            .AddInteractivity()
            .AddPagination();
    })
    .Build();

Snowflake? testGuild = null;
var env = host.Services.GetRequiredService<IHostEnvironment>();
var config = host.Services.GetRequiredService<IConfiguration>();
var logger = host.Services.GetRequiredService<ILogger<Program>>();

Migrate<SpearContext>(host);

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
var updateSlash = await slashService.UpdateSlashCommandsAsync(testGuild);
if(!updateSlash.IsSuccess) {
    logger.LogCritical("Failed to update slash commands: {Reason}", updateSlash.Error?.Message);
    return 1;
}

await host.RunAsync();

return 0;

// https://medium.com/@floyd.may/ef-core-app-migrate-on-startup-d046afdba258
// https://gist.github.com/Tim-Hodge/eea0601a14177c199fe60557eeeff31e
void Migrate<TContext>(IHost host) where TContext : DbContext {
    using var scope = host.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();
    using var ctx = scope.ServiceProvider.GetRequiredService<TContext>();

    var sp = ctx.GetInfrastructure();

    var modelDiffer = sp.GetRequiredService<IMigrationsModelDiffer>();
    var migrationsAssembly = sp.GetRequiredService<IMigrationsAssembly>();

    var modelInitializer = sp.GetRequiredService<IModelRuntimeInitializer>();
    var sourceModel = modelInitializer.Initialize(migrationsAssembly.ModelSnapshot!.Model);

    var designTimeModel = sp.GetRequiredService<IDesignTimeModel>();
    var readOptimizedModel = designTimeModel.Model;

    var diffsExist = modelDiffer.HasDifferences(
        sourceModel.GetRelationalModel(),
        readOptimizedModel.GetRelationalModel());

    if(diffsExist) {
        throw new InvalidOperationException("There are differences between the current database model and the most recent migration.");
    }

    ctx.Database.Migrate();

    // https://www.npgsql.org/efcore/mapping/enum.html#creating-your-database-enum
    using var conn = (NpgsqlConnection) ctx.Database.GetDbConnection();
    conn.Open();
    conn.ReloadTypes();
}
