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
builder.Services.AddSingleton<IEventQueueProcessor, EventQueueProcessor>();

var app = builder.Build();

// Start the event queue processor
var eventQueueProcessor = app.Services.GetRequiredService<IEventQueueProcessor>();
await eventQueueProcessor.StartProcessingAsync();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
