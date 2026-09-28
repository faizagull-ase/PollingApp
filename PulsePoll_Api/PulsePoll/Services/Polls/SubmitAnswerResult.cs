namespace PulsePoll.Services.Polls;

public class SubmitAnswerResult
{
    public bool IsSuccess { get; }
    public int QuestionIndex { get; }
    public int[] Counts { get; }
    public string? RejectReason { get; }

    private SubmitAnswerResult(bool isSuccess, int questionIndex, int[] counts, string? rejectReason)
    {
        IsSuccess = isSuccess;
        QuestionIndex = questionIndex;
        Counts = counts;
        RejectReason = rejectReason;
    }

    public static SubmitAnswerResult Success(int questionIndex, int[] counts) =>
        new(true, questionIndex, counts, null);

    public static SubmitAnswerResult Rejected(string reason) => new(false, 0, Array.Empty<int>(), reason);
}
