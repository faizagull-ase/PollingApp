using Microsoft.EntityFrameworkCore;
using PulsePoll.Models.Entities;

namespace PulsePoll.Data.Repositories;

public class TemplateRepository : ITemplateRepository
{
    private readonly PulsePollDbContext _dbContext;

    public TemplateRepository(PulsePollDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Template> AddAsync(Template template)
    {
        _dbContext.Templates.Add(template);
        await _dbContext.SaveChangesAsync();
        return template;
    }

    public async Task<Template?> GetByIdAsync(int id)
    {
        return await _dbContext.Templates
            .Include(t => t.Questions.OrderBy(q => q.Order))
            .FirstOrDefaultAsync(t => t.Id == id);
    }
}
