namespace PulsePoll.Models.Cache;

public class PollQuestionSnapshot
{
    public string Text { get; set; } = string.Empty;
    public List<string> Options { get; set; } = new();
    public int? CorrectOptionIndex { get; set; }
}

public class PollState
{
    public string PollCode { get; set; } = string.Empty;
    public Guid TemplateId { get; set; }
    public string Status { get; set; } = "Open";
    public int CurrentQuestionIndex { get; set; }
    public List<PollQuestionSnapshot> Questions { get; set; } = new();
    public Dictionary<int, int[]> Tally { get; set; } = new();
    public Dictionary<int, HashSet<string>> AnsweredBy { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
}
