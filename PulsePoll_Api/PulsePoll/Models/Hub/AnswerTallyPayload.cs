namespace PulsePoll.Models.Hub;

public class AnswerTallyPayload
{
    public int QuestionIndex { get; set; }
    public int[] Counts { get; set; } = Array.Empty<int>();
}
