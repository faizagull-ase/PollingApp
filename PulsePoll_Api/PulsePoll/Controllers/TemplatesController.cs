using Microsoft.AspNetCore.Mvc;
using PulsePoll.Models.Dtos.Templates;
using PulsePoll.Services.Templates;

namespace PulsePoll.Controllers;

[ApiController]
[Route("api/templates")]
public class TemplatesController : ControllerBase
{
    private readonly ITemplateService _templateService;

    public TemplatesController(ITemplateService templateService)
    {
        _templateService = templateService;
    }

    [HttpPost]
    public async Task<ActionResult<TemplateResponse>> Create([FromBody] CreateTemplateRequest request)
    {
        var result = await _templateService.CreateAsync(request);

        if (!result.Success)
        {
            return StatusCode(result.StatusCode, new { error = result.Error });
        }

        return StatusCode(result.StatusCode, result.Data);
    }
}
