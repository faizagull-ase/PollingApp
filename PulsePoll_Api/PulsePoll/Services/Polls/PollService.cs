using PulsePoll.Data.Repositories;
using PulsePoll.Exceptions;
using PulsePoll.Models.Cache;
using PulsePoll.Models.Dtos.Polls;
using PulsePoll.Models.Entities;
using PulsePoll.Services.Caching;
using PulsePoll.Services.PollCodes;

namespace PulsePoll.Services.Polls;

public class PollService : IPollService
{
    private readonly ITemplateRepository _templateRepository;
    private readonly IPollRepository _pollRepository;
    private readonly IPollCacheStore _cacheStore;
    private readonly IPollCodeGenerator _codeGenerator;
    private readonly ILogger<PollService> _logger;

    public PollService(
        ITemplateRepository templateRepository,
        IPollRepository pollRepository,
        IPollCacheStore cacheStore,
        IPollCodeGenerator codeGenerator,
        ILogger<PollService> logger)
    {
        _templateRepository = templateRepository;
        _pollRepository = pollRepository;
        _cacheStore = cacheStore;
        _codeGenerator = codeGenerator;
        _logger = logger;
    }

    public async Task<PollResponse> CreatePollAsync(CreatePollRequest request)
    {
        Template? template;
        try
        {
            template = await _templateRepository.GetByIdAsync(request.TemplateId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load template {TemplateId} for poll creation", request.TemplateId);
            throw;
        }

        // Not-found/validation failures are a request/response concern - let them
        // propagate to ExceptionHandlingMiddleware, which maps them to HTTP 404/400.
        if (template is null)
        {
            throw new NotFoundException($"Template {request.TemplateId} was not found.");
        }

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

        Poll dbPoll;
        try
        {
            dbPoll = await _pollRepository.CreateAsync(new Poll
            {
                PollCode = pollCode,
                TemplateId = template.Id,
                Status = PollStatus.Open,
                CurrentQuestionIndex = 0,
                CreatedAt = poll.CreatedAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save poll {PollCode} for template {TemplateId}", pollCode, request.TemplateId);
            throw;
        }

        poll.PollId = dbPoll.Id;

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

    public async Task<NextQuestionResult> NextQuestion(string pollCode)
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

        var outgoingIndex = poll.CurrentQuestionIndex;
        var outgoingQuestion = poll.CurrentQuestion;

        poll.CurrentQuestionIndex++;
        var question = poll.CurrentQuestion;

        if (poll.PollId is int pollId)
        {
            try
            {
                await _pollRepository.AddQuestionResultAsync(new PollQuestionResult
                {
                    PollId = pollId,
                    QuestionIndex = outgoingIndex,
                    Text = outgoingQuestion.Text,
                    Options = outgoingQuestion.Options,
                    CorrectOptionIndex = outgoingQuestion.CorrectOptionIndex,
                    Tally = outgoingQuestion.Tally
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to persist question result for poll {PollCode} index {Index}", pollCode, outgoingIndex);
            }
        }

        return new NextQuestionResult
        {
            Success = true,
            Question = question.Text,
            Options = question.Options,
            QuestionIndex = poll.CurrentQuestionIndex,
            QuestionCount = poll.Questions.Count
        };
    }

    public async Task<ClosePollResult> ClosePoll(string pollCode)
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

        if (poll.PollId is int pollId)
        {
            try
            {
                var persisted = await _pollRepository.GetPersistedQuestionIndexesAsync(pollId);

                for (var index = 0; index < poll.Questions.Count; index++)
                {
                    if (persisted.Contains(index))
                    {
                        continue;
                    }

                    var q = poll.Questions[index];
                    await _pollRepository.AddQuestionResultAsync(new PollQuestionResult
                    {
                        PollId = pollId,
                        QuestionIndex = index,
                        Text = q.Text,
                        Options = q.Options,
                        CorrectOptionIndex = q.CorrectOptionIndex,
                        Tally = q.Tally
                    });
                }

                await _pollRepository.CloseAsync(pollId, poll.ClosedAt.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to persist final results for poll {PollCode}", pollCode);
            }
        }

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
