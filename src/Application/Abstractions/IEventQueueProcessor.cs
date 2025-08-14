using GamificationEngine.Domain.Events;

namespace GamificationEngine.Application.Abstractions;

/// <summary>
/// Interface for processing events from the event queue
/// </summary>
public interface IEventQueueProcessor
{
    /// <summary>
    /// Starts processing events from the queue
    /// </summary>
    /// <param name="cancellationToken">Cancellation token to stop processing</param>
    /// <returns>Task representing the processing operation</returns>
    Task StartProcessingAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Stops processing events from the queue
    /// </summary>
    /// <returns>Task representing the stop operation</returns>
    Task StopProcessingAsync();

    /// <summary>
    /// Gets the current processing status
    /// </summary>
    /// <returns>True if processing is active, false otherwise</returns>
    bool IsProcessing { get; }

    /// <summary>
    /// Gets the number of events processed since start
    /// </summary>
    /// <returns>Count of processed events</returns>
    long ProcessedEventCount { get; }
}