namespace PulsePoll.Models.Entities;

public class Template
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public List<Question> Questions { get; set; } = new();
}
