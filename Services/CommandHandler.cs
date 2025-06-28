using System.Reflection;
using Discord.Commands;
using Discord.WebSocket;
using TrendingDiscordBot.Modules;

namespace TrendingDiscordBot.Services;

public class CommandHandler
{
    private readonly DiscordSocketClient _client;
    private readonly CommandService _commands;
    private readonly IServiceProvider _services;
    private readonly ForwardModule _forwarder;

    // Retrieve client and CommandService instance via ctor
    public CommandHandler(DiscordSocketClient client, CommandService commands, IServiceProvider services, ForwardModule forwarder)
    {
        _commands = commands;
        _client = client;
        _services = services;
        _forwarder = forwarder;
    }

    public async Task InstallCommandsAsync()
    {
        // Hook the MessageReceived event into our command handler
        _client.MessageReceived += HandleCommandAsync;

        // Here we discover all the command modules in the entry 
        // assembly and load them. Starting from Discord.NET 2.0, a
        // service provider is required to be passed into the
        // module registration method to inject the 
        // required dependencies.
        //
        // If you do not use Dependency Injection, pass null.
        // See Dependency Injection guide for more information.
        await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
    }

    private async Task HandleCommandAsync(SocketMessage messageParam)
    {
        if (messageParam is not SocketUserMessage message) return;

        _ = _forwarder.HandleMessage(message);
        
        /*
        var argPos = 0;
         
        var context = new SocketCommandContext(_client, message);
        
        await _commands.ExecuteAsync(
            context,
            argPos,
            _services);
        */
    }
}