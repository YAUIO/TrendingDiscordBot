using Discord.WebSocket;
using TrendingDiscordBot.Modules;

namespace TrendingDiscordBot.Repositories;

public class CachedMessagesRepository
{
    private readonly ForwardModule _module;

    public CachedMessagesRepository(ForwardModule module)
    {
        _module = module;
        CacheClearer();
    }

    public List<SocketMessage> Messages { get; } = new();

    private async Task CacheClearer()
    {
        var expiration = TimeSpan.FromMinutes(4);

        while (true)
        {
            lock (Messages)
            {
                Console.WriteLine("Iteration. Cache size: " + Messages.Count);

                var toRemove = Messages
                    .Where(msg => DateTimeOffset.Now - msg.Timestamp >= expiration)
                    .ToList();

                foreach (var msg in toRemove) Messages.Remove(msg);
            }


            Console.WriteLine("Entering check cycle. Cache size: " + Messages.Count);

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