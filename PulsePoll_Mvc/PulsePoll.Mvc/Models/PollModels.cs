namespace PulsePoll.Mvc.Models;

public class CreatePollRequest
{
    public int TemplateId { get; set; }
}

public class PollResponse
{
    public string PollCode { get; set; } = string.Empty;
    public int QuestionCount { get; set; }
}
