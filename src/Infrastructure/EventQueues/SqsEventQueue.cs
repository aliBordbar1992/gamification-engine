using Amazon.SQS;
using Amazon.SQS.Model;
using GamificationEngine.Application.Abstractions;
using GamificationEngine.Domain.Events;
using GamificationEngine.Domain.Errors;
using GamificationEngine.Shared;
using System.Text.Json;

namespace GamificationEngine.Infrastructure.EventQueues;

/// <summary>
/// AWS SQS implementation of the event queue
/// </summary>
public class SqsEventQueue : IEventQueue, IDisposable
{
    private readonly IAmazonSQS _sqsClient;
    private readonly string _queueUrl;
    private readonly JsonSerializerOptions _jsonOptions;
    private bool _disposed = false;

    public SqsEventQueue(IAmazonSQS sqsClient, string queueUrl)
    {
        _sqsClient = sqsClient ?? throw new ArgumentNullException(nameof(sqsClient));
        _queueUrl = queueUrl ?? throw new ArgumentException("Queue URL cannot be empty", nameof(queueUrl));

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }

    public async Task<Result<bool, DomainError>> EnqueueAsync(Event @event)
    {
        if (@event == null)
            return Result<bool, DomainError>.Failure(new InvalidEventError("Event cannot be null"));

        try
        {
            var eventJson = JsonSerializer.Serialize(@event, _jsonOptions);

            var request = new SendMessageRequest
            {
                QueueUrl = _queueUrl,
                MessageBody = eventJson,
                MessageAttributes = new Dictionary<string, MessageAttributeValue>
                {
                    { "EventType", new MessageAttributeValue { StringValue = @event.EventType, DataType = "String" } },
                    { "UserId", new MessageAttributeValue { StringValue = @event.UserId, DataType = "String" } },
                    { "Timestamp", new MessageAttributeValue { StringValue = @event.Timestamp.ToString("O"), DataType = "String" } }
                }
            };

            await _sqsClient.SendMessageAsync(request);

            return Result<bool, DomainError>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool, DomainError>.Failure(new EventStorageError($"Failed to enqueue event to SQS: {ex.Message}"));
        }
    }

    public async Task<Event?> DequeueAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new ReceiveMessageRequest
            {
                QueueUrl = _queueUrl,
                MaxNumberOfMessages = 1,
                WaitTimeSeconds = 20, // Long polling
                MessageAttributeNames = new List<string> { "All" }
            };

            var response = await _sqsClient.ReceiveMessageAsync(request, cancellationToken);

            if (response.Messages.Count > 0)
            {
                var message = response.Messages[0];

                try
                {
                    var @event = JsonSerializer.Deserialize<Event>(message.Body, _jsonOptions);

                    // Delete the message from the queue after successful processing
                    await _sqsClient.DeleteMessageAsync(_queueUrl, message.ReceiptHandle, cancellationToken);

                    return @event;
                }
                catch (JsonException)
                {
                    // Delete corrupted message
                    await _sqsClient.DeleteMessageAsync(_queueUrl, message.ReceiptHandle, cancellationToken);
                    return null;
                }
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

    public async Task<int> GetQueueSize()
    {
        try
        {
            var request = new GetQueueAttributesRequest
            {
                QueueUrl = _queueUrl,
                AttributeNames = new List<string> { "ApproximateNumberOfMessages" }
            };

            var response = await _sqsClient.GetQueueAttributesAsync(request);

            if (response.Attributes.TryGetValue("ApproximateNumberOfMessages", out var countStr) &&
                int.TryParse(countStr, out var count))
            {
                return count;
            }

            return 0;
        }
        catch
        {
            return -1; // Indicate that size is not available
        }
    }

    public async Task<bool> IsEmpty()
    {
        var size = await GetQueueSize();
        return size == 0;
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _sqsClient?.Dispose();
            _disposed = true;
        }
    }
}
