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
        var response = await _pollService.CreatePollAsync(request);
        return CreatedAtAction(nameof(Create), new { pollCode = response.PollCode }, response);
    }
}
