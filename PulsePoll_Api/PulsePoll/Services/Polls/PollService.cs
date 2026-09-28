using PulsePoll.Data.Repositories;
using PulsePoll.Exceptions;
using PulsePoll.Models.Cache;
using PulsePoll.Models.Dtos.Polls;
using PulsePoll.Models.Hub;
using PulsePoll.PollCodes;
using PulsePoll.Services.Caching;

namespace PulsePoll.Services.Polls;

public class PollService : IPollService
{
    
    private static readonly TimeSpan PollCacheTtl = TimeSpan.FromHours(24);

    private readonly ITemplateRepository _templateRepository;
    private readonly IPollCacheStore _pollCacheStore;
    private readonly PollCodeGenerator _pollCodeGenerator;

    public PollService(
        ITemplateRepository templateRepository,
        IPollCacheStore pollCacheStore,
        PollCodeGenerator pollCodeGenerator)
    {
        _templateRepository = templateRepository;
        _pollCacheStore = pollCacheStore;
        _pollCodeGenerator = pollCodeGenerator;
    }

    public async Task<CreatePollResponse> StartPollAsync(CreatePollRequest request)

    {
        // template Id is required
        if (request.TemplateId == Guid.Empty)
        {
            throw new ValidationFailedException("templateId is required.");
        }
        // get the template from database by template id
        var template = await _templateRepository.GetByIdAsync(request.TemplateId);
        // if template not found , throw exception 
        if (template is null)
        {
            throw new ResourceNotFoundException("Template not found.");
        }
        // generate some random code for session 
        var pollCode = _pollCodeGenerator.Generate();
        // Creating the cache for current PollState
        var state = new PollState
        {
            PollCode = pollCode,
            TemplateId = template.Id,
            Status = "Open",
            CurrentQuestionIndex = 0,
            Questions = template.Questions
                .OrderBy(q => q.Order)
                .Select(q => new PollQuestionSnapshot
                {
                    Text = q.Text,
                    Options = new List<string>(q.Options),
                    CorrectOptionIndex = q.CorrectOptionIndex,
                })
                .ToList(),
            CreatedAt = DateTime.UtcNow,
        };

        for (var i = 0; i < state.Questions.Count; i++)
        {
            state.Tally[i] = new int[state.Questions[i].Options.Count];
            state.AnsweredBy[i] = new HashSet<string>();
        }
        // storing the Poll state cache 

        _pollCacheStore.Set(pollCode, state, PollCacheTtl);

        // Returning the Poll Response DTO 
        return new CreatePollResponse
        {
            PollCode = pollCode,
            QuestionCount = state.Questions.Count,
        };
    }

    public Task<JoinPollResult> JoinPollAsync(string pollCode)
    {
        // IF EMPTY STRING ENTERED
        if (string.IsNullOrWhiteSpace(pollCode))
        {
            return Task.FromResult(JoinPollResult.Rejected("invalid_code"));
        }
        // IF NO SESSION EXISTS WITH THIS POLL CODE
        var state = _pollCacheStore.Get(pollCode);
        // STATE NOT KNOWN
        if (state is null)
        {
            return Task.FromResult(JoinPollResult.Rejected("invalid_code"));
        }
        // POLL CLOSED
        if (state.Status != "Open")
        {
            return Task.FromResult(JoinPollResult.Rejected("poll_closed"));
        }

        return Task.FromResult(JoinPollResult.Success(BuildJoinedPayload(state)));
    }

    public Task<JoinPollResult> GetSnapshotAsync(string pollCode)
    {
        if (string.IsNullOrWhiteSpace(pollCode))
        {
            return Task.FromResult(JoinPollResult.Rejected("invalid_code"));
        }

        var state = _pollCacheStore.Get(pollCode);
        if (state is null)
        {
            return Task.FromResult(JoinPollResult.Rejected("invalid_code"));
        }

        return Task.FromResult(JoinPollResult.Success(BuildJoinedPayload(state)));
    }

    public Task<NextQuestionResult> NextQuestionAsync(string pollCode)
    {
        var state = _pollCacheStore.Get(pollCode);
        
        if (state is null)
        {
            return Task.FromResult(NextQuestionResult.Rejected("invalid_code"));
        }

        if (state.Status != "Open")
        {
            return Task.FromResult(NextQuestionResult.Rejected("poll_closed"));
        }

        if (state.CurrentQuestionIndex >= state.Questions.Count - 1)
        {
            return Task.FromResult(NextQuestionResult.Rejected("already_last_question"));
        }

        state.CurrentQuestionIndex++;
        var question = state.Questions[state.CurrentQuestionIndex];

        var payload = new QuestionChangedPayload
        {
            Question = question.Text,
            Options = new List<string>(question.Options),
            QuestionIndex = state.CurrentQuestionIndex,
            QuestionCount = state.Questions.Count,
        };

        return Task.FromResult(NextQuestionResult.Success(payload));
    }

    public Task<ClosePollResult> ClosePollAsync(string pollCode)
    {
        
        var state = _pollCacheStore.Get(pollCode);
        // If pollcode is not present in the cache  
        if (state is null)
        {
            return Task.FromResult(ClosePollResult.Rejected("invalid_code"));
        }
        // if poll code is present but it's status is closed 
        if (state.Status != "Open")
        {
            return Task.FromResult(ClosePollResult.Rejected("poll_closed"));
        }
        // if neither of above happens close the poll 
        state.Status = "Closed";
        state.ClosedAt = DateTime.UtcNow;
        // final tally before we make the closure
        var finalTally = new int[state.Questions.Count][];
        var perQuestionCorrect = new int?[state.Questions.Count];
        // On all the questions we are copying the tally value + each question's correct index
        for (var i = 0; i < state.Questions.Count; i++)
        {
            finalTally[i] = state.Tally.TryGetValue(i, out var tally)
                ? (int[])tally.Clone()
                : new int[state.Questions[i].Options.Count];
            perQuestionCorrect[i] = state.Questions[i].CorrectOptionIndex;
        }
        
        var payload = new PollClosedPayload
        {
            FinalTally = finalTally,
            PerQuestionCorrect = perQuestionCorrect,
        };

        return Task.FromResult(ClosePollResult.Success(payload));
    }

    public Task<SubmitAnswerResult> SubmitAnswerAsync(string pollCode, string connectionId, int questionIndex, int optionIndex)
    {
        // If the poll state exists or not in memory 
        var state = _pollCacheStore.Get(pollCode);
        if (state is null || state.Status != "Open")
        {
            return Task.FromResult(SubmitAnswerResult.Rejected("poll_closed"));
            
        }
        // if the question index is not valid 
        if (questionIndex != state.CurrentQuestionIndex)
        {
            return Task.FromResult(SubmitAnswerResult.Rejected("stale_question"));
        }
        // it's for invalid option if the option is not from selected ones 
        var question = state.Questions[questionIndex];
        if (optionIndex < 0 || optionIndex >= question.Options.Count)
        {
            return Task.FromResult(SubmitAnswerResult.Rejected("invalid_option"));
        }
        // Here we are storing connectionIds with each question index
        if (!state.AnsweredBy.TryGetValue(questionIndex, out var answeredBy))
        {
            answeredBy = new HashSet<string>();
            state.AnsweredBy[questionIndex] = answeredBy;
        }
        // If set already has connectionId we reject that request 
        if (!answeredBy.Add(connectionId))
        {
            return Task.FromResult(SubmitAnswerResult.Rejected("duplicate"));
        }
        // Here we are storing options count per question 
        if (!state.Tally.TryGetValue(questionIndex, out var tally))
        {
            tally = new int[question.Options.Count];
            state.Tally[questionIndex] = tally;
        }
        // We are pushing optionidex to increment tally count for that option 
        tally[optionIndex]++;

        return Task.FromResult(SubmitAnswerResult.Success(questionIndex, (int[])tally.Clone()));
    }

    private static PollJoinedPayload BuildJoinedPayload(PollState state)
    {
        var questionIndex = state.CurrentQuestionIndex;
        var question = state.Questions[questionIndex];
        var currentTally = state.Tally.TryGetValue(questionIndex, out var tally)
            ? (int[])tally.Clone()
            : new int[question.Options.Count];

        return new PollJoinedPayload
        {
            Question = question.Text,
            Options = new List<string>(question.Options),
            QuestionIndex = questionIndex,
            QuestionCount = state.Questions.Count,
            CurrentTally = currentTally,
        };
    }
}
