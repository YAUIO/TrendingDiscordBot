using Discord;
using Discord.WebSocket;

namespace TrendingDiscordBot.Modules;

public class ForwardModule
{
    private const int Threshold = 4;
    private const ulong Channel = 1388552600646848673;
    
    private readonly DiscordSocketClient _client;

    public ForwardModule(DiscordSocketClient client)
    {
        _client = client;
    }

    public async Task HandleMessage(SocketMessage msg)
    {
        //if (msg.Reactions.Count < Threshold) return; //TODO cache check

        if (_client.GetChannel(Channel) is IMessageChannel destinationChannel)
        {
            await destinationChannel.SendMessageAsync(msg.Content); //TODO implement compiling image or a forward mimicking
        }
    }
}