using PulsePoll.Models.Hub;

namespace PulsePoll.Services.Polls;

public class JoinPollResult
{
    public bool IsSuccess { get; }
    public PollJoinedPayload? Payload { get; }
    public string? RejectReason { get; }

    private JoinPollResult(bool isSuccess, PollJoinedPayload? payload, string? rejectReason)
    {
        IsSuccess = isSuccess;
        Payload = payload;
        RejectReason = rejectReason;
    }

    public static JoinPollResult Success(PollJoinedPayload payload) => new(true, payload, null);

    public static JoinPollResult Rejected(string reason) => new(false, null, reason);
}
