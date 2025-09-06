using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using GamificationEngine.Application.Abstractions;
using GamificationEngine.Domain.Events;
using GamificationEngine.Domain.Errors;
using GamificationEngine.Shared;
using System.Text;
using System.Text.Json;

namespace GamificationEngine.Infrastructure.EventQueues;

/// <summary>
/// RabbitMQ implementation of the event queue
/// </summary>
public class RabbitMqEventQueue : IEventQueue, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly string _queueName;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly EventingBasicConsumer _consumer;
    private readonly QueueingBasicConsumer _queueingConsumer;
    private bool _disposed = false;

    public RabbitMqEventQueue(string connectionString, string queueName)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string cannot be empty", nameof(connectionString));

        if (string.IsNullOrWhiteSpace(queueName))
            throw new ArgumentException("Queue name cannot be empty", nameof(queueName));

        _queueName = queueName;

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        // Parse connection string (format: amqp://username:password@host:port/vhost)
        var factory = new ConnectionFactory
        {
            Uri = new Uri(connectionString)
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        // Declare the queue
        _channel.QueueDeclare(
            queue: _queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        // Set up consumer
        _consumer = new EventingBasicConsumer(_channel);
        _queueingConsumer = new QueueingBasicConsumer(_channel);
    }

    public async Task<Result<bool, DomainError>> EnqueueAsync(Event @event)
    {
        if (@event == null)
            return Result<bool, DomainError>.Failure(new InvalidEventError("Event cannot be null"));

        try
        {
            var eventJson = JsonSerializer.Serialize(@event, _jsonOptions);
            var body = Encoding.UTF8.GetBytes(eventJson);

            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true;
            properties.MessageId = @event.EventId;
            properties.Timestamp = new AmqpTimestamp(@event.Timestamp.ToUnixTimeSeconds());
            properties.Headers = new Dictionary<string, object>
            {
                { "eventType", @event.EventType },
                { "userId", @event.UserId }
            };

            _channel.BasicPublish(
                exchange: "",
                routingKey: _queueName,
                basicProperties: properties,
                body: body);

            return Result<bool, DomainError>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool, DomainError>.Failure(new EventStorageError($"Failed to enqueue event to RabbitMQ: {ex.Message}"));
        }
    }

    public async Task<Event?> DequeueAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Use BasicGet for non-blocking consumption
            var result = _channel.BasicGet(_queueName, autoAck: false);

            if (result != null)
            {
                try
                {
                    var body = Encoding.UTF8.GetString(result.Body.ToArray());
                    var @event = JsonSerializer.Deserialize<Event>(body, _jsonOptions);

                    // Acknowledge the message
                    _channel.BasicAck(result.DeliveryTag, false);

                    return @event;
                }
                catch (JsonException)
                {
                    // Reject corrupted message
                    _channel.BasicNack(result.DeliveryTag, false, false);
                    return null;
                }
            }

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
        try
        {
            var result = _channel.QueueDeclarePassive(_queueName);
            return (int)result.MessageCount;
        }
        catch
        {
            return -1; // Indicate that size is not available
        }
    }

    public bool IsEmpty()
    {
        var size = GetQueueSize();
        return size == 0;
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _channel?.Close();
            _channel?.Dispose();
            _connection?.Close();
            _connection?.Dispose();
            _disposed = true;
        }
    }
}
