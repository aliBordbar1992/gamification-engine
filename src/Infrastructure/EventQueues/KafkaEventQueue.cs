using Confluent.Kafka;
using GamificationEngine.Application.Abstractions;
using GamificationEngine.Domain.Events;
using GamificationEngine.Domain.Errors;
using GamificationEngine.Shared;
using System.Text.Json;

namespace GamificationEngine.Infrastructure.EventQueues;

/// <summary>
/// Kafka implementation of the event queue
/// </summary>
public class KafkaEventQueue : IEventQueue, IDisposable
{
    private readonly IProducer<string, string> _producer;
    private readonly IConsumer<string, string> _consumer;
    private readonly string _topicName;
    private readonly JsonSerializerOptions _jsonOptions;
    private bool _disposed = false;

    public KafkaEventQueue(string bootstrapServers, string topicName, string? consumerGroupId = null)
    {
        if (string.IsNullOrWhiteSpace(bootstrapServers))
            throw new ArgumentException("Bootstrap servers cannot be empty", nameof(bootstrapServers));

        if (string.IsNullOrWhiteSpace(topicName))
            throw new ArgumentException("Topic name cannot be empty", nameof(topicName));

        _topicName = topicName;

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        // Configure producer
        var producerConfig = new ProducerConfig
        {
            BootstrapServers = bootstrapServers,
            Acks = Acks.All,
            Retries = 3,
            EnableIdempotence = true
        };

        _producer = new ProducerBuilder<string, string>(producerConfig).Build();

        // Configure consumer
        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = bootstrapServers,
            GroupId = consumerGroupId ?? "gamification-engine-consumer",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = true,
            EnableAutoOffsetStore = true
        };

        _consumer = new ConsumerBuilder<string, string>(consumerConfig).Build();
        _consumer.Subscribe(topicName);
    }

    public async Task<Result<bool, DomainError>> EnqueueAsync(Event @event)
    {
        if (@event == null)
            return Result<bool, DomainError>.Failure(new InvalidEventError("Event cannot be null"));

        try
        {
            var eventJson = JsonSerializer.Serialize(@event, _jsonOptions);
            var message = new Message<string, string>
            {
                Key = @event.UserId,
                Value = eventJson,
                Headers = new Headers
                {
                    { "eventType", System.Text.Encoding.UTF8.GetBytes(@event.EventType) },
                    { "timestamp", System.Text.Encoding.UTF8.GetBytes(@event.Timestamp.ToString("O")) }
                }
            };

            var result = await _producer.ProduceAsync(_topicName, message);

            return Result<bool, DomainError>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool, DomainError>.Failure(new EventStorageError($"Failed to enqueue event to Kafka: {ex.Message}"));
        }
    }

    public async Task<Event?> DequeueAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var consumeResult = _consumer.Consume(cancellationToken);

            if (consumeResult?.Message?.Value != null)
            {
                var @event = JsonSerializer.Deserialize<Event>(consumeResult.Message.Value, _jsonOptions);
                return @event;
            }

            return null;
        }
        catch (OperationCanceledException)
        {
            return null;
        }
        catch (Exception)
        {
            // Log error but don't throw to avoid breaking the consumer loop
            return null;
        }
    }

    public int GetQueueSize()
    {
        // Kafka doesn't provide a direct way to get queue size
        // This would require additional Kafka admin operations
        return -1; // Indicate that size is not available
    }

    public bool IsEmpty()
    {
        // Kafka doesn't provide a direct way to check if queue is empty
        // This would require additional Kafka admin operations
        return false; // Conservative approach - assume not empty
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _producer?.Dispose();
            _consumer?.Dispose();
            _disposed = true;
        }
    }
}
