using Microsoft.AspNetCore.SignalR;
using PulsePoll.Models.Hub;
using PulsePoll.Services.Polls;

namespace PulsePoll.Hubs;

public class PollHub : Hub
{
    private readonly IPollService _pollService;

    public PollHub(IPollService pollService)
    {
        _pollService = pollService;
    }

    public async Task JoinPoll(string pollCode)
    {
        // Service that is checking if pollcode is valid or not

        var result = await _pollService.JoinPollAsync(pollCode);

        if (!result.IsSuccess)
        {
            await Clients.Caller.SendAsync("JoinRejected", new JoinRejectedPayload { Reason = result.RejectReason! });
            Console.WriteLine("We have not connection established");
            return;
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, pollCode);
        Console.WriteLine("We have connection established");
        await Clients.Caller.SendAsync("PollJoined", result.Payload);
    }

    public async Task SubmitAnswer(string pollCode, int questionIndex, int optionIndex)
    {
        var result = await _pollService.SubmitAnswerAsync(pollCode, Context.ConnectionId, questionIndex, optionIndex);

        if (!result.IsSuccess)
        {
            await Clients.Caller.SendAsync("AnswerRejected", new AnswerRejectedPayload { Reason = result.RejectReason! });
            return;
        }

        await Clients.Caller.SendAsync("AnswerAccepted", new AnswerAcceptedPayload { QuestionIndex = result.QuestionIndex });

        var tallyPayload = new AnswerTallyPayload { QuestionIndex = result.QuestionIndex, Counts = result.Counts };
        await Clients.Group(pollCode).SendAsync("AnswerTally", tallyPayload);
    }

    public async Task NextQuestion(string pollCode)
    {
        var result = await _pollService.NextQuestionAsync(pollCode);

        if (!result.IsSuccess)
        {
            await Clients.Caller.SendAsync("OperatorRejected", new OperatorActionRejectedPayload { Reason = result.RejectReason! });
            return;
        }

        await Clients.Group(pollCode).SendAsync("QuestionChanged", result.Payload);
    }

    // Anyone who knows the poll code can drive it - no operator auth in this raw setup.
    public async Task ClosePoll(string pollCode)
    {
        var result = await _pollService.ClosePollAsync(pollCode);

        if (!result.IsSuccess)
        {
            await Clients.Caller.SendAsync("OperatorRejected", new OperatorActionRejectedPayload { Reason = result.RejectReason! });
            return;
        }

        await Clients.Group(pollCode).SendAsync("PollClosed", result.Payload);
    }

    public async Task GetPollSnapshot(string pollCode)
    {
        var result = await _pollService.GetSnapshotAsync(pollCode);

        if (!result.IsSuccess)
        {
            await Clients.Caller.SendAsync("JoinRejected", new JoinRejectedPayload { Reason = result.RejectReason! });
            return;
        }
        await Groups.AddToGroupAsync(Context.ConnectionId, pollCode);
        await Clients.Caller.SendAsync("PollSnapshot", result.Payload);
    }
}
