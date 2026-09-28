using PulsePoll.Data.Repositories;
using PulsePoll.Exceptions;
using PulsePoll.Models.Cache;
using PulsePoll.Models.Dtos.Polls;
using PulsePoll.Services.Caching;
using PulsePoll.Services.PollCodes;

namespace PulsePoll.Services.Polls;

public class PollService : IPollService
{
    private readonly ITemplateRepository _templateRepository;
    private readonly IPollCacheStore _cacheStore;
    private readonly IPollCodeGenerator _codeGenerator;

    public PollService(
        ITemplateRepository templateRepository,
        IPollCacheStore cacheStore,
        IPollCodeGenerator codeGenerator)
    {
        _templateRepository = templateRepository;
        _cacheStore = cacheStore;
        _codeGenerator = codeGenerator;
    }

    public async Task<PollResponse> CreatePollAsync(CreatePollRequest request)
    {
        var template = await _templateRepository.GetByIdAsync(request.TemplateId)
            ?? throw new NotFoundException($"Template {request.TemplateId} was not found.");

        if (template.Questions.Count == 0)
        {
            throw new ValidationException("Template has no questions.");
        }

        var pollCode = _codeGenerator.Generate();

        var poll = new PollState
        {
            PollCode = pollCode,
            TemplateId = template.Id,
            Status = PollStatus.Open,
            CurrentQuestionIndex = 0,
            CreatedAt = DateTime.UtcNow,
            Questions = template.Questions
                .OrderBy(q => q.Order)
                .Select(q => new PollQuestionState
                {
                    Text = q.Text,
                    Options = q.Options,
                    CorrectOptionIndex = q.CorrectOptionIndex,
                    Tally = new int[q.Options.Count]
                })
                .ToList()
        };

        _cacheStore.Set(poll);

        return new PollResponse
        {
            PollCode = pollCode,
            QuestionCount = poll.Questions.Count
        };
    }

    public JoinPollResult Join(string pollCode)
    {
        var poll = _cacheStore.Get(pollCode);
        if (poll is null)
        {
            return JoinPollResult.Rejected("invalid_code");
        }

        if (poll.Status == PollStatus.Closed)
        {
            return JoinPollResult.Rejected("poll_closed");
        }

        return JoinPollResult.FromPoll(poll);
    }

    public SubmitAnswerResult SubmitAnswer(string pollCode, string connectionId, int questionIndex, int optionIndex)
    {
        var poll = _cacheStore.Get(pollCode);
        if (poll is null)
        {
            return SubmitAnswerResult.Rejected("invalid_code");
        }

        if (poll.Status == PollStatus.Closed)
        {
            return SubmitAnswerResult.Rejected("poll_closed");
        }

        if (questionIndex != poll.CurrentQuestionIndex)
        {
            return SubmitAnswerResult.Rejected("stale_question");
        }

        var question = poll.CurrentQuestion;

        if (optionIndex < 0 || optionIndex >= question.Options.Count)
        {
            return SubmitAnswerResult.Rejected("invalid_option");
        }

        if (!question.AnsweredBy.Add(connectionId))
        {
            return SubmitAnswerResult.Rejected("duplicate");
        }

        question.Tally[optionIndex]++;

        return SubmitAnswerResult.Accepted(questionIndex, question.Tally);
    }

    public NextQuestionResult NextQuestion(string pollCode)
    {
        var poll = _cacheStore.Get(pollCode);
        if (poll is null)
        {
            return NextQuestionResult.Rejected("invalid_code");
        }

        if (poll.Status == PollStatus.Closed)
        {
            return NextQuestionResult.Rejected("poll_closed");
        }

        if (poll.CurrentQuestionIndex >= poll.Questions.Count - 1)
        {
            return NextQuestionResult.Rejected("no_more_questions");
        }

        poll.CurrentQuestionIndex++;
        var question = poll.CurrentQuestion;

        return new NextQuestionResult
        {
            Success = true,
            Question = question.Text,
            Options = question.Options,
            QuestionIndex = poll.CurrentQuestionIndex,
            QuestionCount = poll.Questions.Count
        };
    }

    public ClosePollResult ClosePoll(string pollCode)
    {
        var poll = _cacheStore.Get(pollCode);
        if (poll is null)
        {
            return ClosePollResult.Rejected("invalid_code");
        }

        if (poll.Status == PollStatus.Closed)
        {
            return ClosePollResult.Rejected("poll_closed");
        }

        poll.Status = PollStatus.Closed;
        poll.ClosedAt = DateTime.UtcNow;

        return new ClosePollResult
        {
            Success = true,
            FinalTally = poll.Questions.Select(q => q.Tally).ToList(),
            PerQuestionCorrect = poll.Questions.Select(q => q.CorrectOptionIndex).ToList()
        };
    }

    public JoinPollResult GetSnapshot(string pollCode)
    {
        return Join(pollCode);
    }
}
