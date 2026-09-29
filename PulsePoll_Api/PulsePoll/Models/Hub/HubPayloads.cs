namespace PulsePoll.Models.Hub;

public class PollJoined
{
    public string Question { get; set; } = string.Empty;
    public List<string> Options { get; set; } = new();
    public int QuestionIndex { get; set; }
    public int QuestionCount { get; set; }
    public int[] CurrentTally { get; set; } = Array.Empty<int>();
}

public class JoinRejected
{
    public string Reason { get; set; } = string.Empty;
}

public class AnswerAccepted
{
    public int QuestionIndex { get; set; }
}

public class AnswerRejected
{
    public string Reason { get; set; } = string.Empty;
}

public class AnswerTally
{
    public int QuestionIndex { get; set; }
    public int[] Counts { get; set; } = Array.Empty<int>();
}

public class QuestionChanged
{
    public string Question { get; set; } = string.Empty;
    public List<string> Options { get; set; } = new();
    public int QuestionIndex { get; set; }
    public int QuestionCount { get; set; }
}

public class PollClosed
{
    public List<int[]> FinalTally { get; set; } = new();
    public List<int?> PerQuestionCorrect { get; set; } = new();
}
