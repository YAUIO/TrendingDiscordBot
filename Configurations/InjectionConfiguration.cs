using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TrendingDiscordBot.Modules;
using TrendingDiscordBot.Services;

namespace TrendingDiscordBot.Configurations;

public static class InjectionConfiguration
{
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

        var bot = await GetDiscordBot(config, configurationRoot);

        var services = new ServiceCollection()
            .AddLogging()
            .AddSingleton<IConfigurationRoot>(configurationRoot)
            .AddSingleton<DiscordSocketClient>(bot)
            .AddSingleton<ForwardModule>()
            .AddSingleton<CommandService>()
            .AddSingleton<LoggingService>()
            .AddSingleton<CommandHandler>()
            .AddSingleton<ForwardInterface>()
            .BuildServiceProvider();

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