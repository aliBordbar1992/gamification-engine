using Microsoft.EntityFrameworkCore;
using GamificationEngine.Domain.Events;
using GamificationEngine.Domain.Rewards;
using GamificationEngine.Domain.Users;
using GamificationEngine.Domain.Entities;
using GamificationEngine.Domain.Wallet;

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
    public DbSet<RewardHistory> RewardHistories { get; set; }
    public DbSet<EventDefinition> EventDefinitions { get; set; }
    public DbSet<Wallet> Wallets { get; set; }
    public DbSet<WalletTransaction> WalletTransactions { get; set; }
    public DbSet<WalletTransfer> WalletTransfers { get; set; }

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

        // Configure RewardHistory entity
        modelBuilder.Entity<RewardHistory>(entity =>
        {
            entity.HasKey(e => e.RewardHistoryId);
            entity.Property(e => e.RewardHistoryId).HasMaxLength(50);
            entity.Property(e => e.UserId).HasMaxLength(50).IsRequired();
            entity.Property(e => e.RewardId).HasMaxLength(50).IsRequired();
            entity.Property(e => e.RewardType).HasMaxLength(50).IsRequired();
            entity.Property(e => e.TriggerEventId).HasMaxLength(50).IsRequired();
            entity.Property(e => e.AwardedAt).IsRequired();
            entity.Property(e => e.Success).IsRequired();
            entity.Property(e => e.Message).HasMaxLength(500);

            // Store details as JSON for flexibility
            entity.Property(e => e.Details)
                .HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                    v => System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new Dictionary<string, object>())
                .HasColumnType("jsonb");

            // Indexes for performance
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.RewardType);
            entity.HasIndex(e => e.AwardedAt);
            entity.HasIndex(e => new { e.UserId, e.RewardType });
            entity.HasIndex(e => new { e.UserId, e.AwardedAt });
        });

        // Configure EventDefinition entity
        modelBuilder.Entity<EventDefinition>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500).IsRequired();

            // Store payload schema as JSON for flexibility
            entity.Property(e => e.PayloadSchema)
                .HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                    v => System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new Dictionary<string, string>())
                .HasColumnType("jsonb");

            // Index for performance
            entity.HasIndex(e => e.Id);
        });

        // Configure Wallet entity
        modelBuilder.Entity<Wallet>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.PointCategoryId });
            entity.Property(e => e.UserId).HasMaxLength(50).IsRequired();
            entity.Property(e => e.PointCategoryId).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Balance).IsRequired();

            // Configure one-to-many relationship with transactions
            entity.HasMany<WalletTransaction>()
                .WithOne()
                .HasForeignKey(t => new { t.UserId, t.PointCategoryId })
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes for performance
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.PointCategoryId);
            entity.HasIndex(e => e.Balance);
        });

        // Configure WalletTransaction entity
        modelBuilder.Entity<WalletTransaction>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasMaxLength(50).IsRequired();
            entity.Property(e => e.UserId).HasMaxLength(50).IsRequired();
            entity.Property(e => e.PointCategoryId).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Amount).IsRequired();
            entity.Property(e => e.Type).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500).IsRequired();
            entity.Property(e => e.ReferenceId).HasMaxLength(50);
            entity.Property(e => e.Metadata).HasMaxLength(1000);
            entity.Property(e => e.Timestamp).IsRequired();

            // Indexes for performance
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.PointCategoryId);
            entity.HasIndex(e => e.Type);
            entity.HasIndex(e => e.Timestamp);
            entity.HasIndex(e => e.ReferenceId);
            entity.HasIndex(e => new { e.UserId, e.PointCategoryId });
            entity.HasIndex(e => new { e.UserId, e.PointCategoryId, e.Timestamp });
        });

        // Configure WalletTransfer entity
        modelBuilder.Entity<WalletTransfer>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasMaxLength(50).IsRequired();
            entity.Property(e => e.FromUserId).HasMaxLength(50).IsRequired();
            entity.Property(e => e.ToUserId).HasMaxLength(50).IsRequired();
            entity.Property(e => e.PointCategoryId).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Amount).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500).IsRequired();
            entity.Property(e => e.ReferenceId).HasMaxLength(50);
            entity.Property(e => e.Metadata).HasMaxLength(1000);
            entity.Property(e => e.Status).IsRequired();
            entity.Property(e => e.Timestamp).IsRequired();
            entity.Property(e => e.FailureReason).HasMaxLength(500);

            // Indexes for performance
            entity.HasIndex(e => e.FromUserId);
            entity.HasIndex(e => e.ToUserId);
            entity.HasIndex(e => e.PointCategoryId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.Timestamp);
            entity.HasIndex(e => e.ReferenceId);
        });
    }
}