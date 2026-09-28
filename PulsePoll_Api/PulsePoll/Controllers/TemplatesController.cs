using Microsoft.AspNetCore.Mvc;
using PulsePoll.Models.Dtos.Templates;
using PulsePoll.Services.Templates;
using PulsePoll.Validation;

namespace PulsePoll.Controllers;

[ApiController]
[Route("api/templates")]
public class TemplatesController : ControllerBase
{
    private readonly ITemplateService _templateService;
    private readonly TemplateValidator _templateValidator;

    public TemplatesController(ITemplateService templateService, TemplateValidator templateValidator)
    {
        _templateService = templateService;
        _templateValidator = templateValidator;
    }

    // POST /api/templates -> 2.1 create from structured payload
    [HttpPost]
    public async Task<ActionResult<TemplateResponse>> Create([FromBody] CreateTemplateRequest request)
    {
        var errors = _templateValidator.Validate(request);
        if (errors.Count > 0)
        {
            foreach (var error in errors)
            {
                ModelState.AddModelError(string.Empty, error);
            }

            return ValidationProblem(ModelState);
        }

        var response = await _templateService.CreateAsync(request);
        return Created($"api/templates/{response.Id}", response);
    }

}
