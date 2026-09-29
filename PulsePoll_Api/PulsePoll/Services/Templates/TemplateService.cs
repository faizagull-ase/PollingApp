using PulsePoll.Data.Repositories;
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

    public async Task<TemplateResponse> CreateAsync(CreateTemplateRequest request)
    {
        // Validation failures are a request/response concern - let them propagate
        // to ExceptionHandlingMiddleware, which maps ValidationException to HTTP 400.
        _validator.Validate(request);

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
            throw;
        }

        return new TemplateResponse
        {
            Id = created.Id,
            Title = created.Title,
            QuestionCount = created.Questions.Count,
            CreatedAt = created.CreatedAt
        };
    }
}
