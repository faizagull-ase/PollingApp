namespace PulsePoll.Models.Hub;

// Reason: "invalid_code" | "poll_closed" | "rate_limited"
public class JoinRejectedPayload
{
    public string Reason { get; set; } = string.Empty;
}
