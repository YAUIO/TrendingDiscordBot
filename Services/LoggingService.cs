using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;

namespace TrendingDiscordBot.Services;

public class LoggingService
{
    private readonly ILogger<LoggingService> _logger;
    
    public LoggingService(DiscordSocketClient client, CommandService command, ILogger<LoggingService> logger)
    {
        client.Log += LogAsync;
        command.Log += LogAsync;
        _logger = logger;
    }

    private Task LogAsync(LogMessage message)
    {
        if (message.Exception is CommandException cmdException)
        {
            _logger.LogError(cmdException, "[Command/{Severity}] {Alias} failed to execute in {Channel}.",
                message.Severity, cmdException.Command.Aliases[0], cmdException.Context.Channel);
        }
        else
        {
            _logger.LogDebug("[General/{Severity}] {Message}", message.Severity, message);
        }

        return Task.CompletedTask;
    }
}