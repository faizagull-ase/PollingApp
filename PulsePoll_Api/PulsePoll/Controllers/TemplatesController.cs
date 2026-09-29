using Microsoft.AspNetCore.Mvc;
using PulsePoll.Exceptions;
using PulsePoll.Models.Dtos.Templates;
using PulsePoll.Services.Templates;

namespace PulsePoll.Controllers;

[ApiController]
[Route("api/templates")]
public class TemplatesController : ControllerBase
{
    private readonly ITemplateService _templateService;
    private readonly ILogger<TemplatesController> _logger;

    public TemplatesController(ITemplateService templateService, ILogger<TemplatesController> logger)
    {
        _templateService = templateService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<TemplateResponse>> Create([FromBody] CreateTemplateRequest request)
    {
        try
        {
            var response = await _templateService.CreateAsync(request);
            return CreatedAtAction(nameof(Create), new { id = response.Id }, response);
        }
        catch (ApiException ex)
        {
            _logger.LogWarning(ex, "API exception: {Message}", ex.Message);
            return StatusCode(ex.StatusCode, new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception creating template");
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "An unexpected error occurred." });
        }
    }
}
