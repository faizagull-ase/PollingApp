using PulsePoll.Models.Cache;

namespace PulsePoll.Models.Entities;

public class Poll
{
    public int Id { get; set; }
    public string PollCode { get; set; } = string.Empty;
    public int TemplateId { get; set; }
    public Template? Template { get; set; }
    public PollStatus Status { get; set; } = PollStatus.Open;
    public int CurrentQuestionIndex { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ClosedAt { get; set; }

    public List<PollQuestionResult> QuestionResults { get; set; } = new();
}
