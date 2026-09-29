using Microsoft.AspNetCore.Mvc;
using PulsePoll.Models.Dtos.Polls;
using PulsePoll.Services.Polls;

namespace PulsePoll.Controllers;

[ApiController]
[Route("api/polls")]
public class PollsController : ControllerBase
{
    private readonly IPollService _pollService;

    public PollsController(IPollService pollService)
    {
        _pollService = pollService;
    }

    [HttpPost]
    public async Task<ActionResult<PollResponse>> Create([FromBody] CreatePollRequest request)
    {
        var result = await _pollService.CreatePollAsync(request);

        if (!result.Success)
        {
            return StatusCode(result.StatusCode, new { error = result.Error });
        }

        return StatusCode(result.StatusCode, result.Data);
    }
}
