namespace PulsePoll.Models.Dtos.Templates;

public class QuestionDto
{
    public string Text { get; set; } = string.Empty;
    public List<string> Options { get; set; } = new();
    public int? CorrectOptionIndex { get; set; }
}
