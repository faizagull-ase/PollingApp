namespace PulsePoll.Models.Dtos.Templates;

public class CreateTemplateRequest
{
    public string Title { get; set; } = string.Empty;
    public List<CreateQuestionRequest> Questions { get; set; } = new();
}

public class CreateQuestionRequest
{
    public string Text { get; set; } = string.Empty;
    public List<string> Options { get; set; } = new();
    public int? CorrectOptionIndex { get; set; }
}
