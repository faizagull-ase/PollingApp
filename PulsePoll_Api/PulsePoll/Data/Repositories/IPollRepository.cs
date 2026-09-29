using PulsePoll.Models.Entities;

namespace PulsePoll.Data.Repositories;

public interface IPollRepository
{
    Task<Poll> CreateAsync(Poll poll);
    Task AddQuestionResultAsync(PollQuestionResult result);
    Task<HashSet<int>> GetPersistedQuestionIndexesAsync(int pollId);
    Task CloseAsync(int pollId, DateTime closedAt);
}
