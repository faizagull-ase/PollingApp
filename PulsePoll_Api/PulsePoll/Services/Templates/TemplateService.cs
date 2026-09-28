using PulsePoll.Data.Repositories;
using PulsePoll.Models.Dtos.Templates;
using PulsePoll.Models.Entities;
using PulsePoll.Validation;

namespace PulsePoll.Services.Templates;

public class TemplateService : ITemplateService
{
    private readonly ITemplateRepository _templateRepository;
    private readonly TemplateValidator _validator;

    public TemplateService(ITemplateRepository templateRepository, TemplateValidator validator)
    {
        _templateRepository = templateRepository;
        _validator = validator;
    }

    public async Task<TemplateResponse> CreateAsync(CreateTemplateRequest request)
    {
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

        var created = await _templateRepository.AddAsync(template);

        return new TemplateResponse
        {
            Id = created.Id,
            Title = created.Title,
            QuestionCount = created.Questions.Count,
            CreatedAt = created.CreatedAt
        };
    }
}
