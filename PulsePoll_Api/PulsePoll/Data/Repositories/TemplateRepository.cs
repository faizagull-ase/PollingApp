using Microsoft.EntityFrameworkCore;
using PulsePoll.Models.Entities;

namespace PulsePoll.Data.Repositories;

public class TemplateRepository : ITemplateRepository
{
    private readonly PulsePollDbContext _db;

    public TemplateRepository(PulsePollDbContext db)
    {
        _db = db;
    }

    public async Task<Template?> GetByIdAsync(Guid id) =>
        await _db.Templates
            .Include(t => t.Questions)
            .FirstOrDefaultAsync(t => t.Id == id);

    public async Task AddAsync(Template template)
    {
        await _db.Templates.AddAsync(template);
        await _db.SaveChangesAsync();
    }
}
