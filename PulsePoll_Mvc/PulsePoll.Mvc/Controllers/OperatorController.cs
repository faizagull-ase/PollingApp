using Microsoft.AspNetCore.Mvc;
using PulsePoll.Mvc.Models;
using PulsePoll.Mvc.Services;

namespace PulsePoll.Mvc.Controllers;

public class OperatorController : Controller
{
    private readonly IPulsePollApiClient _apiClient;

    public OperatorController(IPulsePollApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public IActionResult Index() => View();

    [HttpPost]
    public async Task<IActionResult> CreatePoll([FromBody] CreateTemplateRequest request)
    {
        var templateResult = await _apiClient.CreateTemplateAsync(request);
        if (!templateResult.Success)
        {
            return StatusCode(templateResult.StatusCode, new { error = templateResult.Error });
        }

        var pollResult = await _apiClient.CreatePollAsync(new CreatePollRequest { TemplateId = templateResult.Data!.Id });
        if (!pollResult.Success)
        {
            return StatusCode(pollResult.StatusCode, new { error = pollResult.Error });
        }

        return StatusCode(pollResult.StatusCode, pollResult.Data);
    }
}
