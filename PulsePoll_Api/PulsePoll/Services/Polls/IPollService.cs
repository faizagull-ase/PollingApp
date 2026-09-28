using PulsePoll.Models.Dtos.Polls;

namespace PulsePoll.Services.Polls;

public interface IPollService
{
    Task<CreatePollResponse> StartPollAsync(CreatePollRequest request);
    Task<JoinPollResult> JoinPollAsync(string pollCode);
    Task<JoinPollResult> GetSnapshotAsync(string pollCode);
    Task<NextQuestionResult> NextQuestionAsync(string pollCode);
    Task<ClosePollResult> ClosePollAsync(string pollCode);
    Task<SubmitAnswerResult> SubmitAnswerAsync(string pollCode, string connectionId, int questionIndex, int optionIndex);

}
