using PulsePoll.Models.Entities;

namespace PulsePoll.Data.Repositories;

public interface ITemplateRepository
{
    Task<Template?> GetByIdAsync(Guid id);
    Task AddAsync(Template template);
}
