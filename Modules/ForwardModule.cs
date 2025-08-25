using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;

namespace TrendingDiscordBot.Modules;

public class ForwardModule
{
    private readonly ulong _channel;
    private readonly DiscordSocketClient _client;
    private readonly ulong _server;
    private readonly int _threshold;

    public ForwardModule(DiscordSocketClient client, IConfigurationRoot config)
    {
        _client = client;
        _server = Convert.ToUInt64(config["ServerID"]);
        _channel = Convert.ToUInt64(config["ForwardChannelID"]);
        _threshold = Convert.ToInt32(config["Threshold"]);
        
        Console.WriteLine($"ForwardModule initalized with threshold: {_threshold}");
    }

    public async Task<bool> HandleMessage(SocketMessage msg)
    {
        //Console.WriteLine("Checking message: " + msg.Id + " " + msg.Content + " : " + msg.Reactions.Count);

        if (msg.Reactions.Count < _threshold) return false;

        var uniqueUsers = new HashSet<ulong>();

        foreach (var reaction in msg.Reactions)
        {
            var emoji = reaction.Key;

            var users = await msg.GetReactionUsersAsync(emoji, int.MaxValue).FlattenAsync();

            foreach (var user in users)
                uniqueUsers.Add(user.Id);
        }

        if (uniqueUsers.Count < _threshold) return false;

        if (_client.GetChannel(_channel) is IMessageChannel destinationChannel)
        {
            await destinationChannel
                .SendMessageAsync(
                    $"{msg.Author.Username}({msg.Author.GlobalName}): {msg.Content}",
                    embeds: msg.Embeds.ToArray(),
                    stickers: msg.Stickers.ToArray<ISticker>()
                );
            await destinationChannel
                .SendMessageAsync($"https://discord.com/channels/{_server}/{msg.Channel.Id}/{msg.Id}");
        }

        return true;
    }
}