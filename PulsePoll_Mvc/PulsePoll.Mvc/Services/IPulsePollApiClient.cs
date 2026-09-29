using PulsePoll.Mvc.Models;

namespace PulsePoll.Mvc.Services;

public interface IPulsePollApiClient
{
    Task<PulsePollApiResult<TemplateResponse>> CreateTemplateAsync(CreateTemplateRequest request);

    Task<PulsePollApiResult<PollResponse>> CreatePollAsync(CreatePollRequest request);
}
