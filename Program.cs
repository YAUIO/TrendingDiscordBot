using Microsoft.Extensions.DependencyInjection;
using TrendingDiscordBot.Configurations;
using TrendingDiscordBot.Services;

namespace TrendingDiscordBot;

internal class Program
{
    private static IServiceProvider _serviceProvider;

    private static async Task Main(string[] args)
    {
        _serviceProvider = await InjectionConfiguration.CreateProvider();
        
        _serviceProvider.GetRequiredService<LoggingService>();

        await Task.Delay(-1);
    }
}