using Microsoft.Extensions.Caching.Memory;
using PulsePoll.Models.Cache;

namespace PulsePoll.Services.Caching;

public interface IPollCacheStore
{
    void Set(PollState poll);
    PollState? Get(string pollCode);
}

public class MemoryPollCacheStore : IPollCacheStore
{
    private static readonly TimeSpan Ttl = TimeSpan.FromHours(24);
    private readonly IMemoryCache _cache;

    public MemoryPollCacheStore(IMemoryCache cache)
    {
        _cache = cache;
    }

    public void Set(PollState poll)
    {
        _cache.Set(CacheKey(poll.PollCode), poll, Ttl);
    }

    public PollState? Get(string pollCode)
    {
        return _cache.TryGetValue(CacheKey(pollCode), out PollState? poll) ? poll : null;
    }

    private static string CacheKey(string pollCode) => $"poll:{pollCode.ToUpperInvariant()}";
}
