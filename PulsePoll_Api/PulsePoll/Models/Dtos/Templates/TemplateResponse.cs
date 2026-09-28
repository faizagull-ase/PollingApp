namespace PulsePoll.Models.Dtos.Templates;

public class TemplateResponse
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int QuestionCount { get; set; }
    public DateTime CreatedAt { get; set; }
}
