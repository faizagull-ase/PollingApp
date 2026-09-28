using Microsoft.AspNetCore.Http;
using PulsePoll.Models.Dtos.Templates;

namespace PulsePoll.Services.Templates;

public interface ITemplateService
{
    Task<TemplateResponse> CreateAsync(CreateTemplateRequest request);
    Task<UploadPreviewResponse> ParseUploadAsync(IFormFile file);
    Task<TemplateResponse> ConfirmAsync(ConfirmTemplateRequest request);
    Task DiscardAsync(DiscardTemplateRequest request);
    Task<IReadOnlyList<TemplateSummaryResponse>> GetAllAsync();
    Task<TemplateDetailResponse?> GetByIdAsync(Guid id);
    Task<bool> UpdateAsync(Guid id, CreateTemplateRequest request);
    Task<bool> DeleteAsync(Guid id);
}
