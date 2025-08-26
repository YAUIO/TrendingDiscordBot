using System.Reflection;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using TrendingDiscordBot.Repositories;

namespace TrendingDiscordBot.Services;

public class CommandHandler(
    DiscordSocketClient client,
    CommandService commands,
    IServiceProvider services,
    CachedMessagesRepository repository,
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
                    Console.WriteLine($"Guild {server.Name} is not in the allowed list. Leaving....");
                    await server.LeaveAsync();
                }
        };

        client.JoinedGuild += async (guild) =>
        {
            foreach (var server in client.Guilds)
                if (server.Id != _serverid)
                {
                    Console.WriteLine($"Guild {server.Name} is not in the allowed list. Leaving....");
                    await server.LeaveAsync();
                }
        };
        
        client.MessageReceived += HandleCommandAsync;

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

    private async Task HandleCommandAsync(SocketMessage messageParam)
    {
        if (messageParam is not SocketUserMessage message) return;

        if (message.Author.Id == client.CurrentUser.Id) return;

        var argPos = 0;

        var context = new SocketCommandContext(client, message);

        lock (repository.Messages)
        {
            repository.Messages.Add(message);
        }

        await commands.ExecuteAsync(
            context,
            argPos,
            services);
    }
}