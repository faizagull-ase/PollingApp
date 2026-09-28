namespace PulsePoll.Models.Dtos.Polls;

public class CreatePollResponse
{
    public string PollCode { get; set; } = string.Empty;
    public int QuestionCount { get; set; }
}
