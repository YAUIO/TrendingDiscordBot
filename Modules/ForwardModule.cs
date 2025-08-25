using Discord;
using Discord.WebSocket;

namespace TrendingDiscordBot.Modules;

public class ForwardModule(
    DiscordSocketClient client
)
{
    private const int Threshold = 4;
    private const ulong Channel = 1388552600646848673;
    private const ulong Server = 1051185027054051378;

    public async Task<bool> HandleMessage(SocketMessage msg)
    {
        Console.WriteLine("Checking message: " + msg.Id + " " + msg.Content + " : " + msg.Reactions.Count);

        if (msg.Reactions.Count < Threshold) return false;
        
        var uniqueUsers = new HashSet<ulong>();

        foreach (var reaction in msg.Reactions)
        {
            var emoji = reaction.Key;

            // this returns an async enumerable of users
            var users = await msg.GetReactionUsersAsync(emoji, int.MaxValue).FlattenAsync();

            foreach (var user in users)
                uniqueUsers.Add(user.Id);
        }

        if (uniqueUsers.Count < Threshold) return false;

        if (client.GetChannel(Channel) is IMessageChannel destinationChannel)
        {
            await destinationChannel
                .SendMessageAsync(
                    $"{msg.Author.Username}({msg.Author.GlobalName}): {msg.Content}",
                    embeds: msg.Embeds.ToArray(),
                    stickers: msg.Stickers.ToArray<ISticker>()
                );
            await destinationChannel
                .SendMessageAsync($"https://discord.com/channels/{Server}/{msg.Channel.Id}/{msg.Id}");
        }

        return true;
    }
}