using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TrendingDiscordBot.Modules;
using TrendingDiscordBot.Services;

namespace TrendingDiscordBot.Configurations;

public static class InjectionConfiguration
{
    public static ulong ServerId { get; private set; }
    
    public static async Task<IServiceProvider> CreateProvider()
    {
        var config = new DiscordSocketConfig
        {
            MessageCacheSize = 400,
            GatewayIntents = GatewayIntents.Guilds
                             | GatewayIntents.GuildMessages
                             | GatewayIntents.GuildMessageReactions
                             | GatewayIntents.GuildMembers
                             | GatewayIntents.MessageContent
        };

        var configurationRoot = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("configuration.json", false, true)
            .Build();
        
        ServerId = Convert.ToUInt64(configurationRoot["ServerID"]);

        var bot = await GetDiscordBot(config, configurationRoot);
        var options = new FromJsonMemCacheEntryOptions(Convert.ToInt32(configurationRoot["Timeout"]));
        
        var logLevel = configurationRoot["Logging:LogLevel"] ?? "Information";

        var services = new ServiceCollection()
            .AddLogging(builder =>
            {
                builder.ClearProviders();
                builder.AddConsole();
                builder.SetMinimumLevel(Enum.Parse<LogLevel>(logLevel, true));
            })
            .AddMemoryCache()
            .AddSingleton<IConfigurationRoot>(configurationRoot)
            .AddSingleton<DiscordSocketClient>(bot)
            .AddSingleton<MemoryCacheEntryOptions>(options)
            .AddSingleton<ForwardModule>()
            .AddSingleton<CommandService>()
            .AddSingleton<LoggingService>()
            .AddSingleton<CommandHandler>()
            .AddSingleton<ForwardInterface>()
            .BuildServiceProvider();

        Console.WriteLine($"Logging level: {logLevel}");

        return services;
    }

    private static async Task<DiscordSocketClient> GetDiscordBot(DiscordSocketConfig cfg, IConfigurationRoot configRoot)
    {
        var bot = new DiscordSocketClient(cfg);

        var token = configRoot["APIKey"];

        await bot.LoginAsync(TokenType.Bot, token);
        await bot.StartAsync();

        return bot;
    }
}