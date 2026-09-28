using Microsoft.Extensions.Caching.Memory;
using PulsePoll.Models.Cache;

namespace PulsePoll.Services.Caching;

public class MemoryPollCacheStore : IPollCacheStore
{
    private const string KeyPrefix = "poll:";

    private readonly IMemoryCache _cache;

    public MemoryPollCacheStore(IMemoryCache cache)
    {
        _cache = cache;
    }

    public PollState? Get(string pollCode) => _cache.TryGetValue(Key(pollCode), out PollState? state) ? state : null;

    public void Set(string pollCode, PollState state, TimeSpan ttl) => _cache.Set(Key(pollCode), state, ttl);

    public void Remove(string pollCode) => _cache.Remove(Key(pollCode));

    private static string Key(string pollCode) => KeyPrefix + pollCode;
}
