using Microsoft.AspNetCore.Http;
using PulsePoll.Data.Repositories;
using PulsePoll.Models.Dtos.Templates;

namespace PulsePoll.Services.Templates;

public class TemplateService : ITemplateService
{
    private readonly ITemplateRepository _templateRepository;

    public TemplateService(ITemplateRepository templateRepository)
    {
        _templateRepository = templateRepository;
    }

    public async Task<TemplateResponse> CreateAsync(CreateTemplateRequest request)
    {
        var template = new PulsePoll.Models.Entities.Template
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            CreatedAt = DateTime.UtcNow,
        };

        template.Questions = request.Questions.Select((q, index) => new PulsePoll.Models.Entities.Question
        {
            Id = Guid.NewGuid(),
            TemplateId = template.Id,
            Order = index,
            Text = q.Text,
            Options = q.Options,
            CorrectOptionIndex = q.CorrectOptionIndex,
        }).ToList();

        await _templateRepository.AddAsync(template);

        return new TemplateResponse
        {
            Id = template.Id,
            Title = template.Title,
            QuestionCount = template.Questions.Count,
            CreatedAt = template.CreatedAt,
        };
    }

    public Task<UploadPreviewResponse> ParseUploadAsync(IFormFile file) => throw new NotImplementedException();

    public Task<TemplateResponse> ConfirmAsync(ConfirmTemplateRequest request) => throw new NotImplementedException();

    public Task DiscardAsync(DiscardTemplateRequest request) => throw new NotImplementedException();

    public Task<IReadOnlyList<TemplateSummaryResponse>> GetAllAsync() => throw new NotImplementedException();

    public Task<TemplateDetailResponse?> GetByIdAsync(Guid id) => throw new NotImplementedException();

    public Task<bool> UpdateAsync(Guid id, CreateTemplateRequest request) => throw new NotImplementedException();

    public Task<bool> DeleteAsync(Guid id) => throw new NotImplementedException();
}
