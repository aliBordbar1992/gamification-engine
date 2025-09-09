using GamificationEngine.Application.Services;
using GamificationEngine.Domain.Entities;
using GamificationEngine.Domain.Events;
using GamificationEngine.Domain.Repositories;
using Microsoft.Extensions.Options;
using Moq;
using Shouldly;
using Xunit;

namespace GamificationEngine.Application.Tests.Services;

/// <summary>
/// Unit tests for EventValidationService
/// </summary>
public class EventValidationServiceTests
{
    private readonly Mock<IEventDefinitionRepository> _mockEventDefinitionRepository;
    private readonly Mock<IOptions<GamificationEngine.Application.Configuration.EngineConfiguration>> _mockConfiguration;
    private readonly EventValidationService _service;

    public EventValidationServiceTests()
    {
        _mockEventDefinitionRepository = new Mock<IEventDefinitionRepository>();
        _mockConfiguration = new Mock<IOptions<GamificationEngine.Application.Configuration.EngineConfiguration>>();

        var config = new GamificationEngine.Application.Configuration.EngineConfiguration
        {
            Engine = new GamificationEngine.Application.Configuration.EngineSettings
            {
                EventValidation = new GamificationEngine.Application.Configuration.EventValidationSettings
                {
                    Enabled = true,
                    RejectUnknownEvents = true,
                    ValidatePayloadSchema = true
                }
            }
        };

        _mockConfiguration.Setup(x => x.Value).Returns(config);

        _service = new EventValidationService(_mockEventDefinitionRepository.Object, _mockConfiguration.Object);
    }

    [Fact]
    public async Task ValidateEventAsync_WithNullEvent_ShouldReturnFalse()
    {
        // Act
        var result = await _service.ValidateEventAsync(null!);

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public async Task ValidateEventAsync_WithUnknownEventType_ShouldReturnFalse()
    {
        // Arrange
        var @event = new Event("event1", "UNKNOWN_EVENT", "user1", DateTimeOffset.UtcNow);
        _mockEventDefinitionRepository.Setup(x => x.ExistsAsync("UNKNOWN_EVENT", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _service.ValidateEventAsync(@event);

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public async Task ValidateEventAsync_WithKnownEventTypeButInvalidPayload_ShouldReturnFalse()
    {
        // Arrange
        var @event = new Event("event1", "USER_COMMENTED", "user1", DateTimeOffset.UtcNow,
            new Dictionary<string, object> { { "invalidField", "value" } });

        var eventDefinition = new EventDefinition("USER_COMMENTED", "User commented",
            new Dictionary<string, string> { { "commentId", "string" }, { "postId", "string" } });

        _mockEventDefinitionRepository.Setup(x => x.ExistsAsync("USER_COMMENTED", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _mockEventDefinitionRepository.Setup(x => x.GetByIdAsync("USER_COMMENTED", It.IsAny<CancellationToken>()))
            .ReturnsAsync(eventDefinition);

        // Act
        var result = await _service.ValidateEventAsync(@event);

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public async Task ValidateEventAsync_WithKnownEventTypeAndValidPayload_ShouldReturnTrue()
    {
        // Arrange
        var @event = new Event("event1", "USER_COMMENTED", "user1", DateTimeOffset.UtcNow,
            new Dictionary<string, object> { { "commentId", "comment123" }, { "postId", "post456" } });

        var eventDefinition = new EventDefinition("USER_COMMENTED", "User commented",
            new Dictionary<string, string> { { "commentId", "string" }, { "postId", "string" } });

        _mockEventDefinitionRepository.Setup(x => x.ExistsAsync("USER_COMMENTED", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _mockEventDefinitionRepository.Setup(x => x.GetByIdAsync("USER_COMMENTED", It.IsAny<CancellationToken>()))
            .ReturnsAsync(eventDefinition);

        // Act
        var result = await _service.ValidateEventAsync(@event);

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public async Task ValidateEventAsync_WithValidationDisabled_ShouldReturnTrue()
    {
        // Arrange
        var config = new GamificationEngine.Application.Configuration.EngineConfiguration
        {
            Engine = new GamificationEngine.Application.Configuration.EngineSettings
            {
                EventValidation = new GamificationEngine.Application.Configuration.EventValidationSettings
                {
                    Enabled = false
                }
            }
        };

        _mockConfiguration.Setup(x => x.Value).Returns(config);
        var service = new EventValidationService(_mockEventDefinitionRepository.Object, _mockConfiguration.Object);

        var @event = new Event("event1", "UNKNOWN_EVENT", "user1", DateTimeOffset.UtcNow);

        // Act
        var result = await service.ValidateEventAsync(@event);

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public async Task ValidateEventAsync_WithRejectUnknownEventsFalse_ShouldReturnTrue()
    {
        // Arrange
        var config = new GamificationEngine.Application.Configuration.EngineConfiguration
        {
            Engine = new GamificationEngine.Application.Configuration.EngineSettings
            {
                EventValidation = new GamificationEngine.Application.Configuration.EventValidationSettings
                {
                    Enabled = true,
                    RejectUnknownEvents = false
                }
            }
        };

        _mockConfiguration.Setup(x => x.Value).Returns(config);
        var service = new EventValidationService(_mockEventDefinitionRepository.Object, _mockConfiguration.Object);

        var @event = new Event("event1", "UNKNOWN_EVENT", "user1", DateTimeOffset.UtcNow);
        _mockEventDefinitionRepository.Setup(x => x.ExistsAsync("UNKNOWN_EVENT", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await service.ValidateEventAsync(@event);

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public async Task ValidateEventTypeAsync_WithValidEventType_ShouldReturnTrue()
    {
        // Arrange
        _mockEventDefinitionRepository.Setup(x => x.ExistsAsync("USER_COMMENTED", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _service.ValidateEventTypeAsync("USER_COMMENTED");

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public async Task ValidateEventTypeAsync_WithInvalidEventType_ShouldReturnFalse()
    {
        // Arrange
        _mockEventDefinitionRepository.Setup(x => x.ExistsAsync("UNKNOWN_EVENT", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _service.ValidateEventTypeAsync("UNKNOWN_EVENT");

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public async Task ValidateEventTypeAsync_WithEmptyEventType_ShouldReturnFalse()
    {
        // Act
        var result = await _service.ValidateEventTypeAsync("");

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public async Task ValidateEventTypeAsync_WithValidationDisabled_ShouldReturnTrue()
    {
        // Arrange
        var config = new GamificationEngine.Application.Configuration.EngineConfiguration
        {
            Engine = new GamificationEngine.Application.Configuration.EngineSettings
            {
                EventValidation = new GamificationEngine.Application.Configuration.EventValidationSettings
                {
                    Enabled = false
                }
            }
        };

        _mockConfiguration.Setup(x => x.Value).Returns(config);
        var service = new EventValidationService(_mockEventDefinitionRepository.Object, _mockConfiguration.Object);

        // Act
        var result = await service.ValidateEventTypeAsync("UNKNOWN_EVENT");

        // Assert
        result.ShouldBeTrue();
    }
}
