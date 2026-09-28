using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using PulsePoll.Models.Entities;

namespace PulsePoll.Data;

public class PulsePollDbContext : DbContext
{
    public PulsePollDbContext(DbContextOptions<PulsePollDbContext> options) : base(options)
    {
    }

    public DbSet<Template> Templates => Set<Template>();
    public DbSet<Question> Questions => Set<Question>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Template>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Title).IsRequired();

            entity.HasMany(t => t.Questions)
                .WithOne()
                .HasForeignKey(q => q.TemplateId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Question>(entity =>
        {
            entity.HasKey(q => q.Id);
            entity.Property(q => q.Text).IsRequired();

            entity.Property(q => q.Options)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>());
        });
    }
}
