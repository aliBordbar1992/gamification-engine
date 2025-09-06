using GamificationEngine.Api;
using GamificationEngine.Application.Abstractions;
using GamificationEngine.Domain.Repositories;
using GamificationEngine.Infrastructure.Storage.InMemory;
using GamificationEngine.Application.Services;
using Microsoft.OpenApi.Models;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

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

// Register application services
builder.Services.AddScoped<IEventIngestionService, EventIngestionService>();
builder.Services.AddScoped<IUserStateService, UserStateService>();
builder.Services.AddScoped<IRuleManagementService, RuleManagementService>();
builder.Services.AddScoped<IWebhookService, WebhookService>();

// Register infrastructure services
builder.Services.AddSingleton<IEventRepository, EventRepository>();
builder.Services.AddSingleton<IEventQueue, InMemoryEventQueue>();
builder.Services.AddSingleton<IUserStateRepository, InMemoryUserStateRepository>();
builder.Services.AddSingleton<IBadgeRepository, InMemoryBadgeRepository>();
builder.Services.AddSingleton<ITrophyRepository, InMemoryTrophyRepository>();
builder.Services.AddSingleton<ILevelRepository, InMemoryLevelRepository>();
builder.Services.AddSingleton<IPointCategoryRepository, InMemoryPointCategoryRepository>();
builder.Services.AddSingleton<IRuleRepository, InMemoryRuleRepository>();
builder.Services.AddSingleton<IWebhookRepository, InMemoryWebhookRepository>();

// Register HTTP client for webhook notifications
builder.Services.AddHttpClient<IWebhookService, WebhookService>();

// Register background services
builder.Services.AddHostedService<EventQueueBackgroundService>();

// TODO: Uncomment when EF Core project is built
// builder.Services.AddEntityFrameworkStorage(builder.Configuration);
// builder.Services.AddEntityFrameworkHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Gamification Engine API v1");
        c.RoutePrefix = "swagger"; // Set Swagger UI at /swagger
        c.DocumentTitle = "Gamification Engine API Documentation";
        c.DefaultModelsExpandDepth(-1); // Hide models section by default
        c.DisplayRequestDuration();
    });
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// TODO: Uncomment when EF Core project is built
// app.MapHealthChecks("/health");

app.Run();

// Make Program class accessible for testing
public partial class Program { }
