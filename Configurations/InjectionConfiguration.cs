using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TrendingDiscordBot.Services;

namespace TrendingDiscordBot.Configurations;

public class InjectionConfiguration
{
    public static async Task<IServiceProvider> CreateProvider()
    {
        var collection = new ServiceCollection()
            .AddSingleton(await GetDiscordBot())
            .AddSingleton<CommandService>()
            .AddSingleton<LoggingService>();

        return collection.BuildServiceProvider();
    }

    private static async Task<DiscordSocketClient> GetDiscordBot()
    {
        var bot = new DiscordSocketClient();
        var builder = new ConfigurationBuilder()
            .AddUserSecrets<Program>()
            .Build();

        var token = builder["DiscordTestingBotKey"];

        await bot.LoginAsync(TokenType.Bot, token);
        await bot.StartAsync();

        return bot;
    }
}