using Discord;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace TrendingDiscordBot.Modules;

public class ForwardModule
{
    private readonly ulong _channel;
    private readonly ulong _server;
    private readonly int _threshold;
    
    private readonly ForwardInterface _forwarder;
    private readonly ILogger<ForwardModule> _logger;

    public ForwardModule(IConfigurationRoot config, ForwardInterface forwarder, ILogger<ForwardModule> logger)
    {
        _server = Convert.ToUInt64(config["ServerID"]);
        _channel = Convert.ToUInt64(config["ForwardChannelID"]);
        _threshold = Convert.ToInt32(config["Threshold"]);
        _forwarder = forwarder;
        _logger = logger;

        _logger.LogInformation("ForwardModule initialized with threshold: {Threshold}", _threshold);
    }

    public async Task<bool> HandleMessage(IMessage msg)
    {
        _logger.LogDebug("Checking message: {Id} {Content}: {ReactionCount}", msg.Id, msg.Content, msg.Reactions.Count);

        if (msg.Channel.Id == _channel) return false;
        
        _logger.LogDebug("Reactions: {ReactionCount} for \"{Content}\" by {GlobalName}", msg.Reactions.Count, msg.Content, msg.Author.GlobalName);

        if (msg.Reactions.Count < _threshold) return false;

        var uniqueUsers = new HashSet<ulong>();

        foreach (var emoji in msg.Reactions.Select(r => r.Key))
        {
            var users = await msg.GetReactionUsersAsync(emoji, int.MaxValue).FlattenAsync();

            foreach (var user in users)
                uniqueUsers.Add(user.Id);
        }

        _logger.LogDebug("Users: {UserCount} for \"{MsgContent}\" by {GlobalName}", uniqueUsers.Count, msg.Content, msg.Author.GlobalName);

        if (uniqueUsers.Count < _threshold) return false;

        _logger.LogDebug("Sending.... {Content}", msg.Content);
        await _forwarder.Forward(_channel, msg.Id, msg.Channel.Id,_server);

        return true;
    }
}