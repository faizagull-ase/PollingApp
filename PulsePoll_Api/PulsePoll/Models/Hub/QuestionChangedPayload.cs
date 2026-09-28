namespace PulsePoll.Models.Hub;

public class QuestionChangedPayload
{
    public string Question { get; set; } = string.Empty;
    public List<string> Options { get; set; } = new();
    public int QuestionIndex { get; set; }
    public int QuestionCount { get; set; }
}
