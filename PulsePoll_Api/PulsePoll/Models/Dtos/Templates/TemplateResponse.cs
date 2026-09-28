namespace PulsePoll.Models.Dtos.Templates;

// Shape returned by POST /templates and POST /templates/confirm (2.1, 2.3)
public class TemplateResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int QuestionCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

// Full shape returned by GET /templates/{id} (2.5) — includes questions + correctOptionIndex
public class TemplateDetailResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public List<QuestionDto> Questions { get; set; } = new();
}
