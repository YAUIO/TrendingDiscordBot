using System.Reflection;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using TrendingDiscordBot.Configurations;
using TrendingDiscordBot.Modules;

namespace TrendingDiscordBot.Services;

public class CommandHandler(
    DiscordSocketClient client,
    CommandService commands,
    ForwardModule module,
    IServiceProvider services,
    ILogger<CommandHandler> logger,
    IMemoryCache cache,
    MemoryCacheEntryOptions options)
{
    public async Task InstallCommandsAsync()
    {
        // Hook the MessageReceived event into our command handler
        client.Ready += async () =>
        {
            foreach (var server in client.Guilds)
                if (server.Id != InjectionConfiguration.ServerId)
                {
                    logger.LogInformation("Guild {Name} is not in the allowed list. Leaving....", server.Name);
                    await server.LeaveAsync();
                }
        };

        client.JoinedGuild += async (guild) =>
        {
            foreach (var server in client.Guilds)
                if (server.Id != InjectionConfiguration.ServerId)
                {
                    logger.LogInformation("Guild {Name} is not in the allowed list. Leaving....", server.Name);
                    await server.LeaveAsync();
                }
        };

        client.MessageReceived += HandleMessageAsync;

        client.ReactionAdded += HandleReactionAsync;

        // Here we discover all the command modules in the entry 
        // assembly and load them. Starting from Discord.NET 2.0, a
        // service provider is required to be passed into the
        // module registration method to inject the 
        // required dependencies.
        //
        // If you do not use Dependency Injection, pass null.
        // See Dependency Injection guide for more information.
        await commands.AddModulesAsync(Assembly.GetEntryAssembly(), services);
    }

    private async Task HandleMessageAsync(SocketMessage socketMessage)
    {
        logger.LogDebug("Message received: {Message}", socketMessage.Content);

        cache.Set(socketMessage.Id, await module.HandleMessage(socketMessage), options);
    }
    
    private async Task HandleReactionAsync(Cacheable<IUserMessage, ulong> cacheable, Cacheable<IMessageChannel, ulong> cacheable1, SocketReaction arg3)
    {
        var message = await cacheable.GetOrDownloadAsync();
        
        if (message is null) return;
        
        if (message.Author.Id == client.CurrentUser.Id) return;

        if (!cache.TryGetValue(message.Id, out bool isHandled) || isHandled)
        {
            if (isHandled) cache.Remove(message.Id);
            return;
        }
        
        logger.LogDebug("Cache value for {Id} is {Value}", message.Id, isHandled);

        cache.Set(message.Id, await module.HandleMessage(message));
    }
}