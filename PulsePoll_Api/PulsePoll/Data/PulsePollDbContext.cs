using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Text.Json;
using PulsePoll.Models.Entities;

namespace PulsePoll.Data;

public class PulsePollDbContext : DbContext
{
    public PulsePollDbContext(DbContextOptions<PulsePollDbContext> options) : base(options)
    {
    }

    public DbSet<Template> Templates => Set<Template>();
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<Poll> Polls => Set<Poll>();
    public DbSet<PollQuestionResult> PollQuestionResults => Set<PollQuestionResult>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Template>(entity =>
        {
            entity.Property(t => t.Title).IsRequired();
            entity.HasMany(t => t.Questions)
                .WithOne(q => q.Template)
                .HasForeignKey(q => q.TemplateId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Question>(entity =>
        {
            entity.Property(q => q.Text).IsRequired();

            entity.Property(q => q.Options)
                .HasConversion(JsonValueConverter<List<string>>())
                .Metadata.SetValueComparer(JsonValueComparer<List<string>>());
        });

        modelBuilder.Entity<Poll>(entity =>
        {
            entity.Property(p => p.PollCode).IsRequired();
            entity.HasIndex(p => p.PollCode).IsUnique();

            entity.Property(p => p.Status).HasConversion<string>();

            entity.HasOne(p => p.Template)
                .WithMany()
                .HasForeignKey(p => p.TemplateId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PollQuestionResult>(entity =>
        {
            entity.Property(r => r.Text).IsRequired();

            entity.Property(r => r.Options)
                .HasConversion(JsonValueConverter<List<string>>())
                .Metadata.SetValueComparer(JsonValueComparer<List<string>>());

            entity.Property(r => r.Tally)
                .HasConversion(JsonValueConverter<int[]>())
                .Metadata.SetValueComparer(JsonValueComparer<int[]>());

            entity.HasIndex(r => new { r.PollId, r.QuestionIndex }).IsUnique();

            entity.HasOne(r => r.Poll)
                .WithMany(p => p.QuestionResults)
                .HasForeignKey(r => r.PollId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static ValueConverter<T, string> JsonValueConverter<T>() => new(
        value => JsonSerializer.Serialize(value, (JsonSerializerOptions?)null),
        json => JsonSerializer.Deserialize<T>(json, (JsonSerializerOptions?)null)!);

    private static ValueComparer<T> JsonValueComparer<T>() => new(
        (a, b) => JsonSerializer.Serialize(a, (JsonSerializerOptions?)null) == JsonSerializer.Serialize(b, (JsonSerializerOptions?)null),
        v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null).GetHashCode(),
        v => JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(v, (JsonSerializerOptions?)null), (JsonSerializerOptions?)null)!);
}
