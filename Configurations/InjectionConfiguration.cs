using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TrendingDiscordBot.Modules;
using TrendingDiscordBot.Repositories;
using TrendingDiscordBot.Services;

namespace TrendingDiscordBot.Configurations;

public class InjectionConfiguration
{
    public static async Task<IServiceProvider> CreateProvider()
    {
        var config = new DiscordSocketConfig
        {
            MessageCacheSize = 100,
            GatewayIntents = GatewayIntents.All
        };


        var collection = new ServiceCollection()
            .AddSingleton(await GetDiscordBot(config))
            .AddSingleton<ForwardModule>()
            .AddSingleton<CommandService>()
            .AddSingleton<LoggingService>()
            .AddSingleton<CommandHandler>()
            .AddSingleton<CachedMessagesRepository>();

        return collection.BuildServiceProvider();
    }

    private static async Task<DiscordSocketClient> GetDiscordBot(DiscordSocketConfig cfg)
    {
        var bot = new DiscordSocketClient(cfg);
        var builder = new ConfigurationBuilder()
            .AddUserSecrets<Program>()
            .Build();

        var token = builder["DiscordTestingBotKey"];

        await bot.LoginAsync(TokenType.Bot, token);
        await bot.StartAsync();

        return bot;
    }
}