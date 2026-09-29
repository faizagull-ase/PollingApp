namespace PulsePoll.Models.Entities;

public class PollQuestionResult
{
    public int Id { get; set; }
    public int PollId { get; set; }
    public Poll? Poll { get; set; }

    public int QuestionIndex { get; set; }
    public string Text { get; set; } = string.Empty;

    /// Stored as JSON in the database.
    public List<string> Options { get; set; } = new();

    public int? CorrectOptionIndex { get; set; }

    /// Vote count per option index, stored as JSON in the database.
    public int[] Tally { get; set; } = Array.Empty<int>();
}
