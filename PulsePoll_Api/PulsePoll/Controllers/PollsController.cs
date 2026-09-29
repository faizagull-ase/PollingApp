using Microsoft.AspNetCore.Mvc;
using PulsePoll.Exceptions;
using PulsePoll.Models.Dtos.Polls;
using PulsePoll.Services.Polls;

namespace PulsePoll.Controllers;

[ApiController]
[Route("api/polls")]
public class PollsController : ControllerBase
{
    private readonly IPollService _pollService;
    private readonly ILogger<PollsController> _logger;

    public PollsController(IPollService pollService, ILogger<PollsController> logger)
    {
        _pollService = pollService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<PollResponse>> Create([FromBody] CreatePollRequest request)
    {
        try
        {
            var response = await _pollService.CreatePollAsync(request);
            return CreatedAtAction(nameof(Create), new { pollCode = response.PollCode }, response);
        }
        catch (ApiException ex)
        {
            _logger.LogWarning(ex, "API exception: {Message}", ex.Message);
            return StatusCode(ex.StatusCode, new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception creating poll");
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "An unexpected error occurred." });
        }
    }
}
