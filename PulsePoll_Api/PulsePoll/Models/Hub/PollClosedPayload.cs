namespace PulsePoll.Models.Hub;

public class PollClosedPayload
{
    public int[][] FinalTally { get; set; } = Array.Empty<int[]>();

    // perQuestionCorrect[i] is null for questions with no correct answer set
    public int?[] PerQuestionCorrect { get; set; } = Array.Empty<int?>();
}
