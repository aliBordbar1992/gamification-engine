using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using GamificationEngine.Domain.Repositories;
using GamificationEngine.Infrastructure.Storage.EntityFramework.Repositories;
using GamificationEngine.Infrastructure.Storage.EntityFramework.Services;

namespace GamificationEngine.Infrastructure.Storage.EntityFramework.Extensions;

/// <summary>
/// Extension methods for configuring EF Core services
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds EF Core with PostgreSQL support to the service collection
    /// </summary>
    public static IServiceCollection AddEntityFrameworkStorage(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Add DbContext
        services.AddDbContext<GamificationEngineDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Connection string 'DefaultConnection' not found in configuration.");
            }

            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorCodesToAdd: null);
            });

            // Enable sensitive data logging in development
            if (configuration.GetValue<bool>("Logging:EnableSensitiveDataLogging", false))
            {
                options.EnableSensitiveDataLogging();
            }
        });

        // Configure retention options
        services.Configure<EventRetentionOptions>(configuration.GetSection("EventRetention"));

        // Register repositories
        services.AddScoped<IEventRepository, EventRepository>();
        services.AddScoped<IUserStateRepository, UserStateRepository>();
        services.AddScoped<IRewardHistoryRepository, RewardHistoryRepository>();

        // Register background services
        services.AddHostedService<EventRetentionService>();

        return services;
    }

    /// <summary>
    /// Adds EF Core health checks
    /// </summary>
    public static IServiceCollection AddEntityFrameworkHealthChecks(
        this IServiceCollection services)
    {
        services.AddHealthChecks()
            .AddDbContextCheck<GamificationEngineDbContext>("Database");

        return services;
    }
}