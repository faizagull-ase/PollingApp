namespace PulsePoll.Models.Cache;

public enum PollStatus
{
    Open,
    Closed
}

public class PollQuestionState
{
    public string Text { get; set; } = string.Empty;
    public List<string> Options { get; set; } = new();
    public int? CorrectOptionIndex { get; set; }

    /// Vote count per option index.
    public int[] Tally { get; set; } = Array.Empty<int>();

    /// SignalR connection ids that have already answered this question.
    public HashSet<string> AnsweredBy { get; set; } = new();
}

public class PollState
{
    public string PollCode { get; set; } = string.Empty;
    public int TemplateId { get; set; }
    public PollStatus Status { get; set; } = PollStatus.Open;
    public int CurrentQuestionIndex { get; set; }
    public List<PollQuestionState> Questions { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? ClosedAt { get; set; }

    public PollQuestionState CurrentQuestion => Questions[CurrentQuestionIndex];
}
