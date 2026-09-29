using Microsoft.AspNetCore.Http;
using PulsePoll.Data.Repositories;
using PulsePoll.Models.Common;
using PulsePoll.Models.Dtos.Templates;
using PulsePoll.Models.Entities;
using PulsePoll.Validation;

namespace PulsePoll.Services.Templates;

public class TemplateService : ITemplateService
{
    private readonly ITemplateRepository _templateRepository;
    private readonly TemplateValidator _validator;
    private readonly ILogger<TemplateService> _logger;

    public TemplateService(
        ITemplateRepository templateRepository,
        TemplateValidator validator,
        ILogger<TemplateService> logger)
    {
        _templateRepository = templateRepository;
        _validator = validator;
        _logger = logger;
    }

    public async Task<ServiceResult<TemplateResponse>> CreateAsync(CreateTemplateRequest request)
    {
        try
        {
            _validator.Validate(request);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation failed: {Message}", ex.Message);
            return ServiceResult<TemplateResponse>.Fail(ex.Message, StatusCodes.Status400BadRequest);
        }

        var template = new Template
        {
            Title = request.Title,
            CreatedAt = DateTime.UtcNow,
            Questions = request.Questions.Select((q, index) => new Question
            {
                Order = index,
                Text = q.Text,
                Options = q.Options,
                CorrectOptionIndex = q.CorrectOptionIndex
            }).ToList()
        };

        Template created;
        try
        {
            created = await _templateRepository.AddAsync(template);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save template {Title}", request.Title);
            return ServiceResult<TemplateResponse>.Fail("An unexpected error occurred.", StatusCodes.Status500InternalServerError);
        }

        var response = new TemplateResponse
        {
            Id = created.Id,
            Title = created.Title,
            QuestionCount = created.Questions.Count,
            CreatedAt = created.CreatedAt
        };

        return ServiceResult<TemplateResponse>.Ok(response, StatusCodes.Status201Created);
    }
}
