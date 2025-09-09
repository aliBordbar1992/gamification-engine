using GamificationEngine.Application.Abstractions;
using GamificationEngine.Application.Configuration;
using GamificationEngine.Domain.Events;
using GamificationEngine.Domain.Repositories;
using Microsoft.Extensions.Options;

namespace GamificationEngine.Application.Services;

/// <summary>
/// Service for validating events against the event catalog
/// </summary>
public class EventValidationService : IEventValidationService
{
    private readonly IEventDefinitionRepository _eventDefinitionRepository;
    private readonly EventValidationSettings _settings;

    public EventValidationService(
        IEventDefinitionRepository eventDefinitionRepository,
        IOptions<EngineConfiguration> configuration)
    {
        _eventDefinitionRepository = eventDefinitionRepository ?? throw new ArgumentNullException(nameof(eventDefinitionRepository));
        _settings = configuration?.Value?.Engine?.EventValidation ?? new EventValidationSettings();
    }

    public async Task<bool> ValidateEventAsync(Event @event)
    {
        if (@event == null)
            return false;

        if (!_settings.Enabled)
            return true; // Validation disabled

        // First validate the event type
        var isValidType = await ValidateEventTypeAsync(@event.EventType);
        if (!isValidType)
            return !_settings.RejectUnknownEvents; // Return false if rejecting unknown events

        // Get the event definition to validate payload schema
        var eventDefinition = await _eventDefinitionRepository.GetByIdAsync(@event.EventType);
        if (eventDefinition == null)
            return !_settings.RejectUnknownEvents;

        // Validate payload against schema if enabled
        if (_settings.ValidatePayloadSchema)
        {
            return eventDefinition.ValidatePayload(@event.Attributes.ToDictionary(kvp => kvp.Key, kvp => kvp.Value));
        }

        return true;
    }

    public async Task<bool> ValidateEventTypeAsync(string eventType)
    {
        if (string.IsNullOrWhiteSpace(eventType))
            return false;

        if (!_settings.Enabled)
            return true; // Validation disabled

        return await _eventDefinitionRepository.ExistsAsync(eventType);
    }
}
