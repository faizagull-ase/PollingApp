using PulsePoll.Models.Entities;

namespace PulsePoll.Data.Repositories;

public interface ITemplateRepository
{
    Task<Template> AddAsync(Template template);
    Task<Template?> GetByIdAsync(int id);
}
