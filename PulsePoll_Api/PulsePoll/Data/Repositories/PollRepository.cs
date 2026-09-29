using Microsoft.EntityFrameworkCore;
using PulsePoll.Models.Entities;

namespace PulsePoll.Data.Repositories;

public class PollRepository : IPollRepository
{
    private readonly PulsePollDbContext _dbContext;

    public PollRepository(PulsePollDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Poll> CreateAsync(Poll poll)
    {
        _dbContext.Polls.Add(poll);
        await _dbContext.SaveChangesAsync();
        return poll;
    }

    public async Task AddQuestionResultAsync(PollQuestionResult result)
    {
        _dbContext.PollQuestionResults.Add(result);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<HashSet<int>> GetPersistedQuestionIndexesAsync(int pollId)
    {
        return (await _dbContext.PollQuestionResults
            .Where(r => r.PollId == pollId)
            .Select(r => r.QuestionIndex)
            .ToListAsync())
            .ToHashSet();
    }

    public async Task CloseAsync(int pollId, DateTime closedAt)
    {
        var poll = await _dbContext.Polls.FindAsync(pollId);
        if (poll is null)
        {
            return;
        }

        poll.Status = Models.Cache.PollStatus.Closed;
        poll.ClosedAt = closedAt;
        await _dbContext.SaveChangesAsync();
    }
}
