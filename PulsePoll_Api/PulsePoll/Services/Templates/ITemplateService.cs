using PulsePoll.Models.Dtos.Templates;

namespace PulsePoll.Services.Templates;

public interface ITemplateService
{
    Task<TemplateResponse> CreateAsync(CreateTemplateRequest request);
}
