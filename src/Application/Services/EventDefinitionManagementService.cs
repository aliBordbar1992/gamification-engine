using GamificationEngine.Application.Abstractions;
using GamificationEngine.Application.DTOs;
using GamificationEngine.Domain.Entities;
using GamificationEngine.Domain.Repositories;
using GamificationEngine.Shared;

namespace GamificationEngine.Application.Services;

/// <summary>
/// Service for managing event definitions
/// </summary>
public sealed class EventDefinitionManagementService : IEventDefinitionManagementService
{
    private readonly IEventDefinitionRepository _eventDefinitionRepository;

    public EventDefinitionManagementService(IEventDefinitionRepository eventDefinitionRepository)
    {
        _eventDefinitionRepository = eventDefinitionRepository ?? throw new ArgumentNullException(nameof(eventDefinitionRepository));
    }

    public async Task<Result<IEnumerable<EventDefinitionDto>, string>> GetAllEventDefinitionsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var eventDefinitions = await _eventDefinitionRepository.GetAllAsync(cancellationToken);
            var dtos = eventDefinitions.Select(MapToDto);
            return Result.Success<IEnumerable<EventDefinitionDto>, string>(dtos);
        }
        catch (Exception ex)
        {
            return Result.Failure<IEnumerable<EventDefinitionDto>, string>($"Failed to get event definitions: {ex.Message}");
        }
    }

    public async Task<Result<EventDefinitionDto, string>> GetEventDefinitionByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(id))
            return Result.Failure<EventDefinitionDto, string>("ID cannot be empty");

        try
        {
            var eventDefinition = await _eventDefinitionRepository.GetByIdAsync(id, cancellationToken);
            if (eventDefinition == null)
                return Result.Failure<EventDefinitionDto, string>($"Event definition with ID '{id}' not found");

            return Result.Success<EventDefinitionDto, string>(MapToDto(eventDefinition));
        }
        catch (Exception ex)
        {
            return Result.Failure<EventDefinitionDto, string>($"Failed to get event definition: {ex.Message}");
        }
    }

    public async Task<Result<EventDefinitionDto, string>> CreateEventDefinitionAsync(CreateEventDefinitionDto dto, CancellationToken cancellationToken = default)
    {
        if (dto == null)
            return Result.Failure<EventDefinitionDto, string>("DTO cannot be null");

        try
        {
            var exists = await _eventDefinitionRepository.ExistsAsync(dto.Id, cancellationToken);
            if (exists)
                return Result.Failure<EventDefinitionDto, string>($"Event definition with ID '{dto.Id}' already exists");

            var eventDefinition = new EventDefinition(
                dto.Id,
                dto.Description,
                dto.PayloadSchema
            );

            await _eventDefinitionRepository.AddAsync(eventDefinition, cancellationToken);
            return Result.Success<EventDefinitionDto, string>(MapToDto(eventDefinition));
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<EventDefinitionDto, string>($"Invalid event definition data: {ex.Message}");
        }
        catch (Exception ex)
        {
            return Result.Failure<EventDefinitionDto, string>($"Failed to create event definition: {ex.Message}");
        }
    }

    public async Task<Result<EventDefinitionDto, string>> UpdateEventDefinitionAsync(string id, UpdateEventDefinitionDto dto, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(id))
            return Result.Failure<EventDefinitionDto, string>("ID cannot be empty");
        if (dto == null)
            return Result.Failure<EventDefinitionDto, string>("DTO cannot be null");

        try
        {
            var eventDefinition = await _eventDefinitionRepository.GetByIdAsync(id, cancellationToken);
            if (eventDefinition == null)
                return Result.Failure<EventDefinitionDto, string>($"Event definition with ID '{id}' not found");

            // Update the event definition properties
            eventDefinition.Description = dto.Description;
            eventDefinition.PayloadSchema = dto.PayloadSchema ?? new Dictionary<string, string>();

            await _eventDefinitionRepository.UpdateAsync(eventDefinition, cancellationToken);
            return Result.Success<EventDefinitionDto, string>(MapToDto(eventDefinition));
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<EventDefinitionDto, string>($"Invalid event definition data: {ex.Message}");
        }
        catch (Exception ex)
        {
            return Result.Failure<EventDefinitionDto, string>($"Failed to update event definition: {ex.Message}");
        }
    }

    public async Task<Result<bool, string>> DeleteEventDefinitionAsync(string id, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(id))
            return Result.Failure<bool, string>("ID cannot be empty");

        try
        {
            var exists = await _eventDefinitionRepository.ExistsAsync(id, cancellationToken);
            if (!exists)
                return Result.Failure<bool, string>($"Event definition with ID '{id}' not found");

            await _eventDefinitionRepository.DeleteAsync(id, cancellationToken);
            return Result.Success<bool, string>(true);
        }
        catch (Exception ex)
        {
            return Result.Failure<bool, string>($"Failed to delete event definition: {ex.Message}");
        }
    }

    public async Task<Result<bool, string>> EventDefinitionExistsAsync(string id, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(id))
            return Result.Failure<bool, string>("ID cannot be empty");

        try
        {
            var exists = await _eventDefinitionRepository.ExistsAsync(id, cancellationToken);
            return Result.Success<bool, string>(exists);
        }
        catch (Exception ex)
        {
            return Result.Failure<bool, string>($"Failed to check event definition existence: {ex.Message}");
        }
    }

    private static EventDefinitionDto MapToDto(EventDefinition eventDefinition)
    {
        return new EventDefinitionDto
        {
            Id = eventDefinition.Id,
            Description = eventDefinition.Description,
            PayloadSchema = eventDefinition.PayloadSchema
        };
    }
}
