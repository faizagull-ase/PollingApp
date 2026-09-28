namespace PulsePoll.Models.Entities;

public class Question
{
    public Guid Id { get; set; }
    public Guid TemplateId { get; set; }
    public int Order { get; set; }
    public string Text { get; set; } = string.Empty;
    public List<string> Options { get; set; } = new();
    public int? CorrectOptionIndex { get; set; }
}
