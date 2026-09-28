namespace PulsePoll.Models.Entities;

public class Question
{
    public int Id { get; set; }
    public int TemplateId { get; set; }
    public Template? Template { get; set; }

    public int Order { get; set; }
    public string Text { get; set; } = string.Empty;

    /// Stored as JSON in the database (2-4 entries).
    public List<string> Options { get; set; } = new();

    public int? CorrectOptionIndex { get; set; }
}
