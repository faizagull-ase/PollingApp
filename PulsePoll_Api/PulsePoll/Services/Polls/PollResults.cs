using PulsePoll.Models.Cache;

namespace PulsePoll.Services.Polls;

public class JoinPollResult
{
    public bool Success { get; init; }
    public string? RejectReason { get; init; }
    public string Question { get; init; } = string.Empty;
    public List<string> Options { get; init; } = new();
    public int QuestionIndex { get; init; }
    public int QuestionCount { get; init; }
    public int[] CurrentTally { get; init; } = Array.Empty<int>();

    public static JoinPollResult Rejected(string reason) => new() { Success = false, RejectReason = reason };

    public static JoinPollResult FromPoll(PollState poll)
    {
        var current = poll.CurrentQuestion;
        return new JoinPollResult
        {
            Success = true,
            Question = current.Text,
            Options = current.Options,
            QuestionIndex = poll.CurrentQuestionIndex,
            QuestionCount = poll.Questions.Count,
            CurrentTally = current.Tally
        };
    }
}

public class SubmitAnswerResult
{
    public bool Success { get; init; }
    public string? RejectReason { get; init; }
    public int QuestionIndex { get; init; }
    public int[] Counts { get; init; } = Array.Empty<int>();

    public static SubmitAnswerResult Rejected(string reason) => new() { Success = false, RejectReason = reason };

    public static SubmitAnswerResult Accepted(int questionIndex, int[] counts) => new()
    {
        Success = true,
        QuestionIndex = questionIndex,
        Counts = counts
    };
}

public class NextQuestionResult
{
    public bool Success { get; init; }
    public string? RejectReason { get; init; }
    public string Question { get; init; } = string.Empty;
    public List<string> Options { get; init; } = new();
    public int QuestionIndex { get; init; }
    public int QuestionCount { get; init; }

    public static NextQuestionResult Rejected(string reason) => new() { Success = false, RejectReason = reason };
}

public class ClosePollResult
{
    public bool Success { get; init; }
    public string? RejectReason { get; init; }
    public List<int[]> FinalTally { get; init; } = new();
    public List<int?> PerQuestionCorrect { get; init; } = new();

    public static ClosePollResult Rejected(string reason) => new() { Success = false, RejectReason = reason };
}
