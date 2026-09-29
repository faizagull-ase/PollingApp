using PulsePoll.Models.Common;
using PulsePoll.Models.Dtos.Templates;

namespace PulsePoll.Services.Templates;

public interface ITemplateService
{
    Task<ServiceResult<TemplateResponse>> CreateAsync(CreateTemplateRequest request);
}
