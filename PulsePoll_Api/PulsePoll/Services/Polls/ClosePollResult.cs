using PulsePoll.Models.Hub;

namespace PulsePoll.Services.Polls;

public class ClosePollResult
{
    public bool IsSuccess { get; }
    public PollClosedPayload? Payload { get; }
    public string? RejectReason { get; }

    private ClosePollResult(bool isSuccess, PollClosedPayload? payload, string? rejectReason)
    {
        IsSuccess = isSuccess;
        Payload = payload;
        RejectReason = rejectReason;
    }

    public static ClosePollResult Success(PollClosedPayload payload) => new(true, payload, null);

    public static ClosePollResult Rejected(string reason) => new(false, null, reason);
}
