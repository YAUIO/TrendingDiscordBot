using System.Reflection;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TrendingDiscordBot.Modules;

namespace TrendingDiscordBot.Services;

public class CommandHandler(
    DiscordSocketClient client,
    CommandService commands,
    ForwardModule module,
    IServiceProvider services,
    ILogger<CommandHandler> logger,
    IConfigurationRoot config)
{
    private readonly ulong _serverid = Convert.ToUInt64(config["ServerID"]);
    public async Task InstallCommandsAsync()
    {
        // Hook the MessageReceived event into our command handler
        client.Ready += async () =>
        {
            foreach (var server in client.Guilds)
                if (server.Id != _serverid)
                {
                    logger.LogInformation("Guild {Name} is not in the allowed list. Leaving....", server.Name);
                    await server.LeaveAsync();
                }
        };

        client.JoinedGuild += async (guild) =>
        {
            foreach (var server in client.Guilds)
                if (server.Id != _serverid)
                {
                    logger.LogInformation("Guild {Name} is not in the allowed list. Leaving....", server.Name);
                    await server.LeaveAsync();
                }
        };

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
    
    private async Task HandleReactionAsync(Cacheable<IUserMessage, ulong> cacheable, Cacheable<IMessageChannel, ulong> cacheable1, SocketReaction arg3)
    {
        var message = await cacheable.GetOrDownloadAsync();
        
        if (message is null) return;
        
        if (message.Author.Id == client.CurrentUser.Id) return;

        await module.HandleMessage(message);
    }
}