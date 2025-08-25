using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace TrendingDiscordBot.Modules;

public class ForwardInterface
{
    private readonly string? _botToken;

    public ForwardInterface(IConfigurationRoot config)
    {
        _botToken = config["APIKey"];
    }

    public async Task Forward(ulong targetChannelId, ulong originalMessageId, ulong originalChannelId,
        ulong originalGuildId)
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bot", _botToken);

        var payload = new
        {
            content = "", // optional text
            message_reference = new
            {
                message_id = originalMessageId.ToString(),
                channel_id = originalChannelId.ToString(),
                guild_id = originalGuildId.ToString(),
                type = 1 // FORWARD
            }
        };

        var json = JsonSerializer.Serialize(payload);
        var request = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await client.PostAsync(
            $"https://discord.com/api/v10/channels/{targetChannelId}/messages",
            request
        );

        if (response.IsSuccessStatusCode)
            Console.WriteLine("Message forwarded successfully!");
        else
            Console.WriteLine($"Failed: {response.StatusCode}\n{await response.Content.ReadAsStringAsync()}");
    }
}