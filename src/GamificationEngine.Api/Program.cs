using GamificationEngine.Api;
using GamificationEngine.Application.Abstractions;
using GamificationEngine.Domain.Repositories;
using GamificationEngine.Infrastructure.Storage.InMemory;
using GamificationEngine.Application.Services;
using GamificationEngine.Infrastructure.Configuration;
using GamificationEngine.Application.DTOs;
using Microsoft.OpenApi.Models;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAdminUI", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Add Swagger/OpenAPI documentation
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Gamification Engine API",
        Version = "v1",
        Description = "A headless, modular, cross-platform gamification engine for awarding points, badges, trophies, levels, leaderboards, and penalties based on user actions.",
        Contact = new OpenApiContact
        {
            Name = "Gamification Engine Team",
            Email = "support@gamificationengine.com"
        },
        License = new OpenApiLicense
        {
            Name = "MIT License",
            Url = new Uri("https://opensource.org/licenses/MIT")
        }
    });

    // Add examples for dry-run endpoint
    c.MapType<DryRunRequestDto>(() => new OpenApiSchema
    {
        Type = "object",
        Properties = new Dictionary<string, OpenApiSchema>
        {
            ["eventId"] = new OpenApiSchema { Type = "string", Example = new Microsoft.OpenApi.Any.OpenApiString("optional-event-id") },
            ["eventType"] = new OpenApiSchema { Type = "string", Example = new Microsoft.OpenApi.Any.OpenApiString("USER_COMMENTED") },
            ["userId"] = new OpenApiSchema { Type = "string", Example = new Microsoft.OpenApi.Any.OpenApiString("user-123") },
            ["occurredAt"] = new OpenApiSchema { Type = "string", Format = "date-time", Example = new Microsoft.OpenApi.Any.OpenApiString("2024-01-15T10:30:00Z") },
            ["attributes"] = new OpenApiSchema
            {
                Type = "object",
                Example = new Microsoft.OpenApi.Any.OpenApiObject
                {
                    ["commentId"] = new Microsoft.OpenApi.Any.OpenApiString("comment-456"),
                    ["postId"] = new Microsoft.OpenApi.Any.OpenApiString("post-789"),
                    ["text"] = new Microsoft.OpenApi.Any.OpenApiString("Great article!")
                }
            }
        }
    });

    c.MapType<DryRunResponseDto>(() => new OpenApiSchema
    {
        Type = "object",
        Properties = new Dictionary<string, OpenApiSchema>
        {
            ["triggerEventId"] = new OpenApiSchema { Type = "string", Example = new Microsoft.OpenApi.Any.OpenApiString("event-123") },
            ["userId"] = new OpenApiSchema { Type = "string", Example = new Microsoft.OpenApi.Any.OpenApiString("user-123") },
            ["eventType"] = new OpenApiSchema { Type = "string", Example = new Microsoft.OpenApi.Any.OpenApiString("USER_COMMENTED") },
            ["rules"] = new OpenApiSchema { Type = "array", Items = new OpenApiSchema { Type = "object" } },
            ["summary"] = new OpenApiSchema { Type = "object" },
            ["evaluatedAt"] = new OpenApiSchema { Type = "string", Format = "date-time" }
        }
    });

    // Include XML comments for better documentation
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }

    // Add security definitions for API key authentication (if needed in the future)
    c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.ApiKey,
        In = ParameterLocation.Header,
        Name = "X-API-Key",
        Description = "API Key needed to access the endpoints"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "ApiKey"
                    }
                },
                new string[] {}
            }
    });
});

builder.Services.AddOpenApi();

// Load YAML configuration first
var configPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "configuration-example.yml");
if (!File.Exists(configPath))
{
    configPath = Path.Combine(Directory.GetCurrentDirectory(), "configuration-example.yml");
}
if (!File.Exists(configPath))
{
    var projectRoot = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", ".."));
    configPath = Path.Combine(projectRoot, "configuration-example.yml");
}

// Configure application settings with YAML configuration
if (File.Exists(configPath))
{
    var yamlLoader = new YamlConfigurationLoader();
    var yamlConfigResult = await yamlLoader.LoadFromFileAsync(configPath);
    if (yamlConfigResult.IsSuccess)
    {
        builder.Services.Configure<GamificationEngine.Application.Configuration.EngineConfiguration>(config =>
        {
            config.Engine = yamlConfigResult.Value?.Engine ?? new();
            config.Events = yamlConfigResult.Value?.Events ?? new();
            config.PointCategories = yamlConfigResult.Value?.PointCategories ?? new();
            config.Badges = yamlConfigResult.Value?.Badges ?? new();
            config.Trophies = yamlConfigResult.Value?.Trophies ?? new();
            config.Levels = yamlConfigResult.Value?.Levels ?? new();
            config.Rules = yamlConfigResult.Value?.Rules ?? new();
            config.Simulation = yamlConfigResult.Value?.Simulation;
        });
    }
}
else
{
    // Fallback to appsettings.json if YAML not found
    builder.Services.Configure<GamificationEngine.Application.Configuration.EngineConfiguration>(builder.Configuration);
}

// Register application services
builder.Services.AddScoped<IEventIngestionService, EventIngestionService>();
builder.Services.AddScoped<IUserStateService, UserStateService>();
builder.Services.AddScoped<ILeaderboardService, LeaderboardService>();
builder.Services.AddScoped<IRuleManagementService, RuleManagementService>();
builder.Services.AddScoped<IEntityManagementService, EntityManagementService>();
builder.Services.AddScoped<IEventDefinitionManagementService, EventDefinitionManagementService>();
builder.Services.AddScoped<IWebhookService, WebhookService>();
builder.Services.AddScoped<IDatabaseSeederService, DatabaseSeederService>();
builder.Services.AddScoped<ISpendingExecutionService, SpendingExecutionService>();
builder.Services.AddScoped<IUserStateSeederService, UserStateSeederService>();
builder.Services.AddScoped<RewardHistorySeederService>();
builder.Services.AddScoped<IWalletService, WalletService>();
builder.Services.AddScoped<IConfigurationLoader, YamlConfigurationLoader>();
builder.Services.AddScoped<IEventValidationService, EventValidationService>();
builder.Services.AddScoped<IRewardExecutionService, RewardExecutionService>();
builder.Services.AddScoped<IRuleEvaluationService, RuleEvaluationService>();
builder.Services.AddScoped<IDryRunEvaluationService, DryRunEvaluationService>();

// Register infrastructure services
builder.Services.AddSingleton<IEventRepository, EventRepository>();
builder.Services.AddSingleton<IEventQueue, InMemoryEventQueue>();
builder.Services.AddSingleton<ILeaderboardRepository, InMemoryLeaderboardRepository>();
builder.Services.AddSingleton<IUserStateRepository, InMemoryUserStateRepository>();
builder.Services.AddSingleton<IRewardHistoryRepository, InMemoryRewardHistoryRepository>();
builder.Services.AddSingleton<IBadgeRepository, InMemoryBadgeRepository>();
builder.Services.AddSingleton<ITrophyRepository, InMemoryTrophyRepository>();
builder.Services.AddSingleton<ILevelRepository, InMemoryLevelRepository>();
builder.Services.AddSingleton<IPointCategoryRepository, InMemoryPointCategoryRepository>();
builder.Services.AddSingleton<IWalletRepository, InMemoryWalletRepository>();
builder.Services.AddSingleton<IWalletTransactionRepository, InMemoryWalletTransactionRepository>();
builder.Services.AddSingleton<IWalletTransferRepository, InMemoryWalletTransferRepository>();
builder.Services.AddSingleton<IRuleRepository, InMemoryRuleRepository>();
builder.Services.AddSingleton<IWebhookRepository, InMemoryWebhookRepository>();
builder.Services.AddSingleton<IEventDefinitionRepository, InMemoryEventDefinitionRepository>();

// Register HTTP client for webhook notifications
builder.Services.AddHttpClient<IWebhookService, WebhookService>();

// Register background services
builder.Services.AddHostedService<EventQueueBackgroundService>();

// TODO: Uncomment when EF Core project is built
// builder.Services.AddEntityFrameworkStorage(builder.Configuration);
// builder.Services.AddEntityFrameworkHealthChecks();

var app = builder.Build();

// Seed database if empty
await SeedDatabaseIfEmptyAsync(app);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Gamification Engine API v1");
        c.RoutePrefix = "swagger"; // Set Swagger UI at /swagger
        c.DocumentTitle = "Gamification Engine API Documentation";
        //c.DefaultModelsExpandDepth(-1); // Hide models section by default
        c.DisplayRequestDuration();
    });
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Enable CORS
app.UseCors("AllowAdminUI");

app.UseAuthorization();
app.MapControllers();

// TODO: Uncomment when EF Core project is built
// app.MapHealthChecks("/health");

app.Run();

// Make Program class accessible for testing
public partial class Program
{
    /// <summary>
    /// Seeds the database with configuration data if it's empty
    /// </summary>
    static async Task SeedDatabaseIfEmptyAsync(WebApplication app)
    {
        try
        {
            using var scope = app.Services.CreateScope();
            var seeder = scope.ServiceProvider.GetRequiredService<IDatabaseSeederService>();

            // Look for configuration file in the project root
            var configPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "configuration-example.yml");
            if (!File.Exists(configPath))
            {
                // Try alternative path
                configPath = Path.Combine(Directory.GetCurrentDirectory(), "configuration-example.yml");
            }

            // Try absolute path from project root
            if (!File.Exists(configPath))
            {
                var projectRoot = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", ".."));
                configPath = Path.Combine(projectRoot, "configuration-example.yml");
            }

            if (File.Exists(configPath))
            {
                Console.WriteLine($"📁 Found configuration file at: {configPath}");
                var result = await seeder.SeedIfEmptyAsync(configPath);
                if (result.IsSuccess && result.Value)
                {
                    Console.WriteLine("✅ Database seeded successfully with configuration data.");
                }
                else if (result.IsSuccess && !result.Value)
                {
                    Console.WriteLine("ℹ️ Database already contains data, skipping seeding.");
                }
                else
                {
                    Console.WriteLine($"⚠️ Failed to seed database: {result.Error}");
                }
            }
            else
            {
                Console.WriteLine($"⚠️ Configuration file not found at: {configPath}");
            }

            // Seed UserState data
            var userStateSeedPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "userstate-seed-data.yml");
            if (!File.Exists(userStateSeedPath))
            {
                // Try alternative path
                userStateSeedPath = Path.Combine(Directory.GetCurrentDirectory(), "userstate-seed-data.yml");
            }

            // Try absolute path from project root
            if (!File.Exists(userStateSeedPath))
            {
                var projectRoot = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", ".."));
                userStateSeedPath = Path.Combine(projectRoot, "userstate-seed-data.yml");
            }

            if (File.Exists(userStateSeedPath))
            {
                Console.WriteLine($"📁 Found UserState seed file at: {userStateSeedPath}");
                var userStateResult = await seeder.SeedUserStatesIfEmptyAsync(userStateSeedPath);
                if (userStateResult.IsSuccess && userStateResult.Value)
                {
                    Console.WriteLine("✅ UserState data seeded successfully.");
                }
                else if (userStateResult.IsSuccess && !userStateResult.Value)
                {
                    Console.WriteLine("ℹ️ UserState data already exists, skipping seeding.");
                }
                else
                {
                    Console.WriteLine($"⚠️ Failed to seed UserState data: {userStateResult.Error}");
                }
            }
            else
            {
                Console.WriteLine($"⚠️ UserState seed file not found at: {userStateSeedPath}");
            }

            // Seed RewardHistory data
            var rewardHistorySeedPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "reward-history-seed-data.yml");
            if (!File.Exists(rewardHistorySeedPath))
            {
                // Try alternative path
                rewardHistorySeedPath = Path.Combine(Directory.GetCurrentDirectory(), "reward-history-seed-data.yml");
            }

            // Try absolute path from project root
            if (!File.Exists(rewardHistorySeedPath))
            {
                var projectRoot = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", ".."));
                rewardHistorySeedPath = Path.Combine(projectRoot, "reward-history-seed-data.yml");
            }

            if (File.Exists(rewardHistorySeedPath))
            {
                Console.WriteLine($"📁 Found RewardHistory seed file at: {rewardHistorySeedPath}");
                using var rewardHistoryScope = app.Services.CreateScope();
                var rewardHistorySeeder = rewardHistoryScope.ServiceProvider.GetRequiredService<RewardHistorySeederService>();
                var rewardHistoryResult = await rewardHistorySeeder.SeedIfEmptyAsync(rewardHistorySeedPath);
                if (rewardHistoryResult.IsSuccess && rewardHistoryResult.Value)
                {
                    Console.WriteLine("✅ RewardHistory data seeded successfully.");
                }
                else if (rewardHistoryResult.IsSuccess && !rewardHistoryResult.Value)
                {
                    Console.WriteLine("ℹ️ RewardHistory data already exists, skipping seeding.");
                }
                else
                {
                    Console.WriteLine($"⚠️ Failed to seed RewardHistory data: {rewardHistoryResult.Error}");
                }
            }
            else
            {
                Console.WriteLine($"⚠️ RewardHistory seed file not found at: {rewardHistorySeedPath}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error during database seeding: {ex.Message}");
        }
    }
}
