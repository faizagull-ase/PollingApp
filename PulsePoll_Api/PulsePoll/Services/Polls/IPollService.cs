using PulsePoll.Models.Dtos.Polls;

namespace PulsePoll.Services.Polls;

public interface IPollService
{
    Task<PollResponse> CreatePollAsync(CreatePollRequest request);
    JoinPollResult Join(string pollCode);
    SubmitAnswerResult SubmitAnswer(string pollCode, string connectionId, int questionIndex, int optionIndex);
    Task<NextQuestionResult> NextQuestion(string pollCode);
    Task<ClosePollResult> ClosePoll(string pollCode);
    JoinPollResult GetSnapshot(string pollCode);
}
