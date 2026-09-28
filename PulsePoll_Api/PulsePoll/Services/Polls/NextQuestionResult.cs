using PulsePoll.Models.Hub;

namespace PulsePoll.Services.Polls;

public class NextQuestionResult
{
    public bool IsSuccess { get; }
    public QuestionChangedPayload? Payload { get; }
    public string? RejectReason { get; }

    private NextQuestionResult(bool isSuccess, QuestionChangedPayload? payload, string? rejectReason)
    {
        IsSuccess = isSuccess;
        Payload = payload;
        RejectReason = rejectReason;
    }

    public static NextQuestionResult Success(QuestionChangedPayload payload) => new(true, payload, null);

    public static NextQuestionResult Rejected(string reason) => new(false, null, reason);
}
