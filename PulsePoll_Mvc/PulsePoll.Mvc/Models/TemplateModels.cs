namespace PulsePoll.Mvc.Models;

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

public class TemplateResponse
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int QuestionCount { get; set; }
    public DateTime CreatedAt { get; set; }
}
