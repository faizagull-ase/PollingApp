using Microsoft.AspNetCore.SignalR;
using PulsePoll.Models.Hub;
using PulsePoll.Services.Polls;

namespace PulsePoll.Hubs;

// NOTE: there is currently no operator authentication - anyone who knows the poll
// code can call NextQuestion/ClosePoll, not just the operator who started the poll.
public class PollHub : Hub
{
    private readonly IPollService _pollService;

    public PollHub(IPollService pollService)
    {
        _pollService = pollService;
    }

    public async Task<object> JoinPoll(string pollCode)
    {
        var result = _pollService.Join(pollCode);

        if (!result.Success)
        {
            return new JoinRejected { Reason = result.RejectReason! };
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, pollCode);

        return new PollJoined
        {
            Question = result.Question,
            Options = result.Options,
            QuestionIndex = result.QuestionIndex,
            QuestionCount = result.QuestionCount,
            CurrentTally = result.CurrentTally
        };
    }

    public async Task<object> SubmitAnswer(string pollCode, int questionIndex, int optionIndex)
    {
        var result = _pollService.SubmitAnswer(pollCode, Context.ConnectionId, questionIndex, optionIndex);

        if (!result.Success)
        {
            return new AnswerRejected { Reason = result.RejectReason! };
        }

        await Clients.Group(pollCode).SendAsync("AnswerTally", new AnswerTally
        {
            QuestionIndex = result.QuestionIndex,
            Counts = result.Counts
        });

        return new AnswerAccepted { QuestionIndex = result.QuestionIndex };
    }

    public async Task NextQuestion(string pollCode)
    {
        var result = _pollService.NextQuestion(pollCode);

        if (!result.Success)
        {
            return;
        }

        await Clients.Group(pollCode).SendAsync("QuestionChanged", new QuestionChanged
        {
            Question = result.Question,
            Options = result.Options,
            QuestionIndex = result.QuestionIndex,
            QuestionCount = result.QuestionCount
        });
    }

    public async Task ClosePoll(string pollCode)
    {
        var result = _pollService.ClosePoll(pollCode);

        if (!result.Success)
        {
            return;
        }

        await Clients.Group(pollCode).SendAsync("PollClosed", new PollClosed
        {
            FinalTally = result.FinalTally,
            PerQuestionCorrect = result.PerQuestionCorrect
        });
    }

    public object GetPollSnapshot(string pollCode)
    {
        var result = _pollService.GetSnapshot(pollCode);

        if (!result.Success)
        {
            return new JoinRejected { Reason = result.RejectReason! };
        }

        return new PollJoined
        {
            Question = result.Question,
            Options = result.Options,
            QuestionIndex = result.QuestionIndex,
            QuestionCount = result.QuestionCount,
            CurrentTally = result.CurrentTally
        };
    }
}
