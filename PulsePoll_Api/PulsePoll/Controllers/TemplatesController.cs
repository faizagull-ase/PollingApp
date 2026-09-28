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
        var response = await _templateService.CreateAsync(request);
        return CreatedAtAction(nameof(Create), new { id = response.Id }, response);
    }
}
