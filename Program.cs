using Microsoft.Extensions.DependencyInjection;
using TrendingDiscordBot.Configurations;
using TrendingDiscordBot.Services;

namespace TrendingDiscordBot;

internal class Program
{
    private static IServiceProvider _serviceProvider;

    private static async Task Main(string[] args)
    {
        Console.WriteLine("Version 26.08.2025 03:00");
        
        _serviceProvider = await InjectionConfiguration.CreateProvider();

        _serviceProvider.GetRequiredService<LoggingService>();

        var handler = _serviceProvider.GetRequiredService<CommandHandler>();
        await handler.InstallCommandsAsync();

        await Task.Delay(-1);
    }
}