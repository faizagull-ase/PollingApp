namespace PulsePoll.Models.Dtos.Templates;

// Shape returned by GET /templates (2.4) — metadata only, no question bodies
public class TemplateSummaryResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int QuestionCount { get; set; }
    public DateTime CreatedAt { get; set; }
}
