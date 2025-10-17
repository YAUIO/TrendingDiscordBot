using Microsoft.Extensions.DependencyInjection;
using TrendingDiscordBot.Configurations;
using TrendingDiscordBot.Services;

namespace TrendingDiscordBot;

internal class Program
{
    private static IServiceProvider _serviceProvider;

    private static async Task Main(string[] args)
    {
        Console.WriteLine("Version 18.10.2025 00:00");
        
        _serviceProvider = await InjectionConfiguration.CreateProvider();

        _serviceProvider.GetRequiredService<LoggingService>();

        var handler = _serviceProvider.GetRequiredService<CommandHandler>();
        await handler.InstallCommandsAsync();

        await Task.Delay(-1);
    }
}