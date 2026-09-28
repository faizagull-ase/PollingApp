namespace PulsePoll.Models.Hub;

public class PollJoinedPayload
{
    public string Question { get; set; } = string.Empty;
    public List<string> Options { get; set; } = new();
    public int QuestionIndex { get; set; }
    public int QuestionCount { get; set; }
    public int[] CurrentTally { get; set; } = Array.Empty<int>();
}
