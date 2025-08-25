using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using TrendingDiscordBot.Modules;

namespace TrendingDiscordBot.Repositories;

public class CachedMessagesRepository
{
    private readonly ForwardModule _module;
    private readonly int _timeout;

    public CachedMessagesRepository(ForwardModule module, IConfigurationRoot config)
    {
        _timeout = Convert.ToInt32(config["Timeout"]);
        _module = module;
        CacheClearer();
        Console.WriteLine($"Repository initalized with timeout: {_timeout}s");
    }

    public List<SocketMessage> Messages { get; } = new();

    private async Task CacheClearer()
    {
        var expiration = TimeSpan.FromSeconds(_timeout);

        while (true)
        {
            lock (Messages)
            {
                //Console.WriteLine("Iteration. Cache size: " + Messages.Count);

                var toRemove = Messages
                    .Where(msg => DateTimeOffset.Now - msg.Timestamp >= expiration)
                    .ToList();

                foreach (var msg in toRemove) Messages.Remove(msg);
            }


            //Console.WriteLine("Entering check cycle. Cache size: " + Messages.Count);

            List<SocketMessage> handled = new();
            foreach (var msg in Messages.ToList()) // snapshot to avoid collection modified
                if (await _module.HandleMessage(msg))
                    handled.Add(msg);

            lock (Messages)
            {
                foreach (var msg in handled)
                    Messages.Remove(msg);
            }


            await Task.Delay(1000);
        }
    }
}