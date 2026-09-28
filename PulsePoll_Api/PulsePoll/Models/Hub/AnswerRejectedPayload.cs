namespace PulsePoll.Models.Hub;

// Reason: "poll_closed" | "duplicate" | "invalid_option" | "stale_question"
public class AnswerRejectedPayload
{
    public string Reason { get; set; } = string.Empty;
}
