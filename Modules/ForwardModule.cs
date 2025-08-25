using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;

namespace TrendingDiscordBot.Modules;

public class ForwardModule
{
    private readonly ulong _channel;
    private readonly ForwardInterface _forwarder;
    private readonly ulong _server;
    private readonly int _threshold;

    public ForwardModule(IConfigurationRoot config, ForwardInterface forwarder)
    {
        _server = Convert.ToUInt64(config["ServerID"]);
        _channel = Convert.ToUInt64(config["ForwardChannelID"]);
        _threshold = Convert.ToInt32(config["Threshold"]);
        _forwarder = forwarder;

        Console.WriteLine($"ForwardModule initalized with threshold: {_threshold}");
    }

    public async Task<bool> HandleMessage(SocketMessage msg)
    {
        //Console.WriteLine("Checking message: " + msg.Id + " " + msg.Content + " : " + msg.Reactions.Count);

        //Console.WriteLine($"Reactions: {msg.Reactions.Count} for \"{msg.Content}\" by {msg.Author.GlobalName}");

        if (msg.Reactions.Count < _threshold) return false;

        var uniqueUsers = new HashSet<ulong>();

        foreach (var reaction in msg.Reactions)
        {
            var emoji = reaction.Key;

            var users = await msg.GetReactionUsersAsync(emoji, int.MaxValue).FlattenAsync();

            foreach (var user in users)
                uniqueUsers.Add(user.Id);
        }

        //Console.WriteLine($"Users: {uniqueUsers.Count} for \"{msg.Content}\" by {msg.Author.GlobalName}");

        if (uniqueUsers.Count < _threshold) return false;

        Console.WriteLine($"Sending.... {msg.Content}");
        await _forwarder.Forward(_channel, msg.Id, msg.Channel.Id,_server);

        return true;
    }
}