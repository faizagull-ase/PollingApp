namespace PulsePoll.Models.Dtos.Templates;

public class CreateTemplateRequest
{
    public string Title { get; set; } = string.Empty;
    public List<QuestionDto> Questions { get; set; } = new();
}
