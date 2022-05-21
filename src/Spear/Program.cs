using Remora.Discord.Gateway;
using Remora.Discord.Hosting.Extensions;

var host = Host
    .CreateDefaultBuilder(args)
    .AddDiscordService(services => services.GetRequiredService<IConfiguration>()["DiscordToken"])
    .ConfigureServices(services => {
        services.Configure<DiscordGatewayClientOptions>(options => {
        });
    })
    .Build();

await host.RunAsync();
