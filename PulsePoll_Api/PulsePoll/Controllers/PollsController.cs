using Microsoft.AspNetCore.Mvc;
using PulsePoll.Services.Polls;
using PulsePoll.Models.Dtos.Polls;
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
     public async Task<ActionResult<CreatePollResponse>> StartPoll([FromBody] CreatePollRequest request)
 {
     var response = await _pollService.StartPollAsync(request);
     return StatusCode(StatusCodes.Status201Created, response);
 }

}
