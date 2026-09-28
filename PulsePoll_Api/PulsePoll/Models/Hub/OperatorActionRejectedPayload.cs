namespace PulsePoll.Models.Hub;

// Reason: "invalid_code" | "poll_closed" | "already_last_question"
public class OperatorActionRejectedPayload
{
    public string Reason { get; set; } = string.Empty;
}
