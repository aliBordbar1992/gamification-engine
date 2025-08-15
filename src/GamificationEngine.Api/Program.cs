using GamificationEngine.Api;
using GamificationEngine.Application.Abstractions;
using GamificationEngine.Domain.Repositories;
using GamificationEngine.Infrastructure.Storage.InMemory;
using GamificationEngine.Application.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// Register application services
builder.Services.AddScoped<IEventIngestionService, EventIngestionService>();

// Register infrastructure services
builder.Services.AddSingleton<IEventRepository, EventRepository>();
builder.Services.AddSingleton<IEventQueue, InMemoryEventQueue>();

// Register background services
builder.Services.AddHostedService<EventQueueBackgroundService>();

// TODO: Uncomment when EF Core project is built
// builder.Services.AddEntityFrameworkStorage(builder.Configuration);
// builder.Services.AddEntityFrameworkHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// TODO: Uncomment when EF Core project is built
// app.MapHealthChecks("/health");

app.Run();
