using PulsePoll.Models.Cache;

namespace PulsePoll.Services.Caching;

public interface IPollCacheStore
{
    PollState? Get(string pollCode);
    void Set(string pollCode, PollState state, TimeSpan ttl);
    void Remove(string pollCode);
}
