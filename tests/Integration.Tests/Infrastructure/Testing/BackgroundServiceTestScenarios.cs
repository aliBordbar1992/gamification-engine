using GamificationEngine.Integration.Tests.Infrastructure.Abstractions;
using GamificationEngine.Integration.Tests.Infrastructure.Utils;
using Microsoft.Extensions.Logging;

namespace GamificationEngine.Integration.Tests.Infrastructure.Testing;

/// <summary>
/// Predefined test scenarios for background service testing
/// </summary>
public static class BackgroundServiceTestScenarios
{
    /// <summary>
    /// Tests the complete lifecycle of a background service
    /// </summary>
    public static async Task<bool> TestServiceLifecycleAsync<TService>(
        ITestBackgroundService service,
        ITestTimingUtilities timingUtilities,
        ILogger logger,
        TimeSpan timeout = default) where TService : class
    {
        if (timeout == default)
            timeout = TimeSpan.FromSeconds(30);

        logger.LogInformation("Testing service lifecycle for {ServiceType}", typeof(TService).Name);

        try
        {
            // Test service start
            logger.LogDebug("Starting service...");
            await service.StartAsync();

            var startResult = await timingUtilities.WaitForConditionAsync(
                () => service.IsRunning,
                timeout);

            if (!startResult)
            {
                logger.LogError("Service failed to start within timeout");
                return false;
            }

            logger.LogDebug("Service started successfully");

            // Test service running state
            if (service.Status != BackgroundServiceStatus.Running)
            {
                logger.LogError("Service status is not Running after start. Current status: {Status}", service.Status);
                return false;
            }

            // Test service stop
            logger.LogDebug("Stopping service...");
            await service.StopAsync();

            var stopResult = await timingUtilities.WaitForConditionAsync(
                () => !service.IsRunning,
                timeout);

            if (!stopResult)
            {
                logger.LogError("Service failed to stop within timeout");
                return false;
            }

            logger.LogDebug("Service stopped successfully");

            // Test service stopped state
            if (service.Status != BackgroundServiceStatus.Stopped)
            {
                logger.LogError("Service status is not Stopped after stop. Current status: {Status}", service.Status);
                return false;
            }

            logger.LogInformation("Service lifecycle test completed successfully");
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Service lifecycle test failed");
            return false;
        }
    }

    /// <summary>
    /// Tests background service processing with multiple items
    /// </summary>
    public static async Task<bool> TestProcessingMultipleItemsAsync<TService>(
        ITestBackgroundService service,
        ITestTimingUtilities timingUtilities,
        ILogger logger,
        Func<Task> enqueueItems,
        Func<long> getProcessedCount,
        int expectedItemCount,
        TimeSpan timeout = default) where TService : class
    {
        if (timeout == default)
            timeout = TimeSpan.FromSeconds(30);

        logger.LogInformation("Testing processing of {ExpectedCount} items for {ServiceType}",
            expectedItemCount, typeof(TService).Name);

        try
        {
            // Start the service
            await service.StartAsync();
            await timingUtilities.WaitForConditionAsync(() => service.IsRunning, timeout);

            // Enqueue items for processing
            logger.LogDebug("Enqueuing {ExpectedCount} items for processing", expectedItemCount);
            await enqueueItems();

            // Wait for processing to complete
            var processingResult = await timingUtilities.WaitForConditionAsync(
                () => getProcessedCount() >= expectedItemCount,
                timeout);

            if (!processingResult)
            {
                logger.LogError("Service did not process all items within timeout");
                return false;
            }

            // Verify final count
            var finalCount = getProcessedCount();
            if (finalCount < expectedItemCount)
            {
                logger.LogError("Service processed {FinalCount} items, expected {ExpectedCount}",
                    finalCount, expectedItemCount);
                return false;
            }

            logger.LogInformation("Processing test completed successfully. Processed {FinalCount} items", finalCount);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Processing test failed");
            return false;
        }
        finally
        {
            // Stop the service
            if (service.IsRunning)
            {
                await service.StopAsync();
            }
        }
    }

    /// <summary>
    /// Tests background service error handling and recovery
    /// </summary>
    public static async Task<bool> TestErrorHandlingAndRecoveryAsync<TService>(
        ITestBackgroundService service,
        ITestTimingUtilities timingUtilities,
        ILogger logger,
        Func<Task> triggerError,
        Func<bool> isServiceHealthy,
        TimeSpan timeout = default) where TService : class
    {
        if (timeout == default)
            timeout = TimeSpan.FromSeconds(30);

        logger.LogInformation("Testing error handling and recovery for {ServiceType}", typeof(TService).Name);

        try
        {
            // Start the service
            await service.StartAsync();
            await timingUtilities.WaitForConditionAsync(() => service.IsRunning, timeout);

            // Verify service is healthy
            if (!isServiceHealthy())
            {
                logger.LogError("Service is not healthy before error test");
                return false;
            }

            // Trigger an error
            logger.LogDebug("Triggering error condition...");
            await triggerError();

            // Wait for service to recover
            var recoveryResult = await timingUtilities.WaitForConditionAsync(
                isServiceHealthy,
                timeout);

            if (!recoveryResult)
            {
                logger.LogError("Service did not recover from error within timeout");
                return false;
            }

            // Verify service is still running
            if (!service.IsRunning)
            {
                logger.LogError("Service is not running after recovery");
                return false;
            }

            logger.LogInformation("Error handling and recovery test completed successfully");
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error handling test failed");
            return false;
        }
        finally
        {
            // Stop the service
            if (service.IsRunning)
            {
                await service.StopAsync();
            }
        }
    }

    /// <summary>
    /// Tests background service performance under load
    /// </summary>
    public static async Task<bool> TestPerformanceUnderLoadAsync<TService>(
        ITestBackgroundService service,
        ITestTimingUtilities timingUtilities,
        ILogger logger,
        Func<Task> enqueueLoad,
        Func<long> getProcessedCount,
        int expectedItemCount,
        TimeSpan maxProcessingTime,
        TimeSpan timeout = default) where TService : class
    {
        if (timeout == default)
            timeout = TimeSpan.FromSeconds(60);

        logger.LogInformation("Testing performance under load for {ServiceType}. Expected: {ExpectedCount} items in {MaxTime}",
            typeof(TService).Name, expectedItemCount, maxProcessingTime);

        try
        {
            // Start the service
            await service.StartAsync();
            await timingUtilities.WaitForConditionAsync(() => service.IsRunning, timeout);

            // Enqueue load
            logger.LogDebug("Enqueuing load of {ExpectedCount} items", expectedItemCount);
            await enqueueLoad();

            // Measure processing time
            var processingResult = await timingUtilities.MeasureExecutionTimeAsync(async () =>
            {
                var result = await timingUtilities.WaitForProcessingCompletionAsync(
                    getProcessedCount,
                    expectedItemCount,
                    timeout);

                if (!result)
                {
                    throw new TimeoutException("Processing did not complete within timeout");
                }
            });

            // Verify performance
            if (processingResult > maxProcessingTime)
            {
                logger.LogError("Processing took {ActualTime}, exceeded maximum {MaxTime}",
                    processingResult, maxProcessingTime);
                return false;
            }

            // Verify all items were processed
            var finalCount = getProcessedCount();
            if (finalCount < expectedItemCount)
            {
                logger.LogError("Service processed {FinalCount} items, expected {ExpectedCount}",
                    finalCount, expectedItemCount);
                return false;
            }

            logger.LogInformation("Performance test completed successfully. Processed {FinalCount} items in {ProcessingTime}",
                finalCount, processingResult);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Performance test failed");
            return false;
        }
        finally
        {
            // Stop the service
            if (service.IsRunning)
            {
                await service.StopAsync();
            }
        }
    }

    /// <summary>
    /// Tests background service graceful shutdown
    /// </summary>
    public static async Task<bool> TestGracefulShutdownAsync<TService>(
        ITestBackgroundService service,
        ITestTimingUtilities timingUtilities,
        ILogger logger,
        Func<Task> enqueueItems,
        Func<long> getProcessedCount,
        int itemCount,
        TimeSpan shutdownTimeout = default) where TService : class
    {
        if (shutdownTimeout == default)
            shutdownTimeout = TimeSpan.FromSeconds(10);

        logger.LogInformation("Testing graceful shutdown for {ServiceType}", typeof(TService).Name);

        try
        {
            // Start the service
            await service.StartAsync();
            await timingUtilities.WaitForConditionAsync(() => service.IsRunning, timeout: TimeSpan.FromSeconds(10));

            // Enqueue some items
            logger.LogDebug("Enqueuing {ItemCount} items for processing", itemCount);
            await enqueueItems();

            // Start processing
            await Task.Delay(100); // Give service time to start processing

            // Stop the service
            logger.LogDebug("Initiating graceful shutdown...");
            var stopTask = service.StopAsync();

            // Wait for shutdown to complete
            var shutdownResult = await timingUtilities.WaitForAllTasksAsync(
                new[] { stopTask },
                shutdownTimeout);

            if (!shutdownResult)
            {
                logger.LogError("Service did not shut down gracefully within timeout");
                return false;
            }

            // Verify service is stopped
            if (service.IsRunning)
            {
                logger.LogError("Service is still running after shutdown");
                return false;
            }

            // Verify final count (some items may have been processed before shutdown)
            var finalCount = getProcessedCount();
            logger.LogInformation("Graceful shutdown test completed. Processed {FinalCount} items before shutdown", finalCount);

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Graceful shutdown test failed");
            return false;
        }
    }

    /// <summary>
    /// Tests background service concurrent operations
    /// </summary>
    public static async Task<bool> TestConcurrentOperationsAsync<TService>(
        ITestBackgroundService service,
        ITestTimingUtilities timingUtilities,
        ILogger logger,
        Func<Task>[] concurrentOperations,
        TimeSpan timeout = default) where TService : class
    {
        if (timeout == default)
            timeout = TimeSpan.FromSeconds(30);

        logger.LogInformation("Testing concurrent operations for {ServiceType}. Operations: {OperationCount}",
            typeof(TService).Name, concurrentOperations.Length);

        try
        {
            // Start the service
            await service.StartAsync();
            await timingUtilities.WaitForConditionAsync(() => service.IsRunning, timeout);

            // Execute concurrent operations
            logger.LogDebug("Executing {OperationCount} concurrent operations", concurrentOperations.Length);
            var concurrentTasks = concurrentOperations.Select(op => op()).ToArray();

            // Wait for all operations to complete
            var completionResult = await timingUtilities.WaitForAllTasksAsync(
                concurrentTasks,
                timeout);

            if (!completionResult)
            {
                logger.LogError("Not all concurrent operations completed within timeout");
                return false;
            }

            // Verify service is still healthy
            if (!service.IsRunning)
            {
                logger.LogError("Service is not running after concurrent operations");
                return false;
            }

            logger.LogInformation("Concurrent operations test completed successfully");
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Concurrent operations test failed");
            return false;
        }
        finally
        {
            // Stop the service
            if (service.IsRunning)
            {
                await service.StopAsync();
            }
        }
    }
}