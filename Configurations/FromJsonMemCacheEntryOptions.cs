using Microsoft.Extensions.Caching.Memory;

namespace TrendingDiscordBot.Configurations;

public class FromJsonMemCacheEntryOptions : MemoryCacheEntryOptions
{
    public FromJsonMemCacheEntryOptions(int timeout)
    : base()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(timeout);
    }
}