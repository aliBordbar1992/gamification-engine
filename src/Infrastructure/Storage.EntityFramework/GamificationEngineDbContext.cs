using Microsoft.EntityFrameworkCore;
using GamificationEngine.Domain.Events;
using GamificationEngine.Domain.Users;

namespace GamificationEngine.Infrastructure.Storage.EntityFramework;

/// <summary>
/// Main DbContext for the Gamification Engine using EF Core with PostgreSQL
/// </summary>
public class GamificationEngineDbContext : DbContext
{
    public GamificationEngineDbContext(DbContextOptions<GamificationEngineDbContext> options) : base(options)
    {
    }

    public DbSet<Event> Events { get; set; }
    public DbSet<UserState> UserStates { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Event entity
        modelBuilder.Entity<Event>(entity =>
        {
            entity.HasKey(e => e.EventId);
            entity.Property(e => e.EventId).HasMaxLength(50);
            entity.Property(e => e.EventType).HasMaxLength(100).IsRequired();
            entity.Property(e => e.UserId).HasMaxLength(50).IsRequired();
            entity.Property(e => e.OccurredAt).IsRequired();

            // Store attributes as JSON for flexibility
            entity.Property(e => e.Attributes)
                .HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                    v => System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new Dictionary<string, object>())
                .HasColumnType("jsonb");

            // Indexes for performance
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.EventType);
            entity.HasIndex(e => e.OccurredAt);
            entity.HasIndex(e => new { e.UserId, e.EventType });
        });

        // Configure UserState entity
        modelBuilder.Entity<UserState>(entity =>
        {
            entity.HasKey(e => e.UserId);
            entity.Property(e => e.UserId).HasMaxLength(50);

            // Store points as JSON for flexibility across categories
            entity.Property(e => e.PointsByCategory)
                .HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                    v => System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, long>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new Dictionary<string, long>())
                .HasColumnType("jsonb");

            // Store badge and trophy collections as JSON arrays
            entity.Property(e => e.Badges)
                .HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                    v => System.Text.Json.JsonSerializer.Deserialize<HashSet<string>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new HashSet<string>())
                .HasColumnType("jsonb");

            entity.Property(e => e.Trophies)
                .HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                    v => System.Text.Json.JsonSerializer.Deserialize<HashSet<string>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new HashSet<string>())
                .HasColumnType("jsonb");
        });
    }
}