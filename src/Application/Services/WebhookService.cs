using System.Text;
using System.Text.Json;
using GamificationEngine.Application.Abstractions;
using GamificationEngine.Application.DTOs;
using GamificationEngine.Domain.Repositories;
using GamificationEngine.Shared;

namespace GamificationEngine.Application.Services;

/// <summary>
/// Service for managing webhook notifications
/// </summary>
public class WebhookService : IWebhookService
{
    private readonly HttpClient _httpClient;
    private readonly IWebhookRepository _webhookRepository;

    public WebhookService(HttpClient httpClient, IWebhookRepository webhookRepository)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _webhookRepository = webhookRepository ?? throw new ArgumentNullException(nameof(webhookRepository));
    }

    public async Task<Result<WebhookDto, string>> RegisterWebhookAsync(RegisterWebhookDto webhookDto, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(webhookDto.Id))
                return Result<WebhookDto, string>.Failure("Webhook ID cannot be empty");

            if (string.IsNullOrWhiteSpace(webhookDto.Name))
                return Result<WebhookDto, string>.Failure("Webhook name cannot be empty");

            if (string.IsNullOrWhiteSpace(webhookDto.Url))
                return Result<WebhookDto, string>.Failure("Webhook URL cannot be empty");

            if (!Uri.TryCreate(webhookDto.Url, UriKind.Absolute, out _))
                return Result<WebhookDto, string>.Failure("Invalid webhook URL");

            if (!webhookDto.EventTypes.Any())
                return Result<WebhookDto, string>.Failure("Webhook must have at least one event type");

            // Check if webhook already exists
            var exists = await _webhookRepository.ExistsAsync(webhookDto.Id, cancellationToken);
            if (exists)
                return Result<WebhookDto, string>.Failure("Webhook with this ID already exists");

            // TODO: Implement webhook registration logic
            // This would involve creating the domain Webhook object and storing it
            // For now, return a placeholder
            return Result<WebhookDto, string>.Failure("Webhook registration not yet implemented");
        }
        catch (Exception ex)
        {
            return Result<WebhookDto, string>.Failure($"Failed to register webhook: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<WebhookDto>, string>> GetAllWebhooksAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // TODO: Implement webhook retrieval logic
            // This would involve getting all webhooks from the repository
            // For now, return empty collection
            return Result<IEnumerable<WebhookDto>, string>.Success(new List<WebhookDto>());
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<WebhookDto>, string>.Failure($"Failed to get webhooks: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<WebhookDto>, string>> GetWebhooksByEventTypeAsync(string eventType, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(eventType))
                return Result<IEnumerable<WebhookDto>, string>.Failure("Event type cannot be empty");

            // TODO: Implement webhook retrieval by event type logic
            // This would involve filtering webhooks by event type
            // For now, return empty collection
            return Result<IEnumerable<WebhookDto>, string>.Success(new List<WebhookDto>());
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<WebhookDto>, string>.Failure($"Failed to get webhooks by event type: {ex.Message}");
        }
    }

    public async Task<Result<WebhookDto, string>> GetWebhookByIdAsync(string webhookId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(webhookId))
                return Result<WebhookDto, string>.Failure("Webhook ID cannot be empty");

            // TODO: Implement webhook retrieval by ID logic
            // This would involve getting the webhook from the repository
            // For now, return not found
            return Result<WebhookDto, string>.Failure("Webhook not found");
        }
        catch (Exception ex)
        {
            return Result<WebhookDto, string>.Failure($"Failed to get webhook by ID: {ex.Message}");
        }
    }

    public async Task<Result<WebhookDto, string>> UpdateWebhookAsync(string webhookId, UpdateWebhookDto webhookDto, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(webhookId))
                return Result<WebhookDto, string>.Failure("Webhook ID cannot be empty");

            if (string.IsNullOrWhiteSpace(webhookDto.Name))
                return Result<WebhookDto, string>.Failure("Webhook name cannot be empty");

            if (string.IsNullOrWhiteSpace(webhookDto.Url))
                return Result<WebhookDto, string>.Failure("Webhook URL cannot be empty");

            if (!Uri.TryCreate(webhookDto.Url, UriKind.Absolute, out _))
                return Result<WebhookDto, string>.Failure("Invalid webhook URL");

            if (!webhookDto.EventTypes.Any())
                return Result<WebhookDto, string>.Failure("Webhook must have at least one event type");

            // TODO: Implement webhook update logic
            // This would involve updating the webhook in the repository
            // For now, return not found
            return Result<WebhookDto, string>.Failure("Webhook not found");
        }
        catch (Exception ex)
        {
            return Result<WebhookDto, string>.Failure($"Failed to update webhook: {ex.Message}");
        }
    }

    public async Task<Result<bool, string>> DeleteWebhookAsync(string webhookId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(webhookId))
                return Result<bool, string>.Failure("Webhook ID cannot be empty");

            // TODO: Implement webhook deletion logic
            // This would involve deleting the webhook from the repository
            // For now, return not found
            return Result<bool, string>.Failure("Webhook not found");
        }
        catch (Exception ex)
        {
            return Result<bool, string>.Failure($"Failed to delete webhook: {ex.Message}");
        }
    }

    public async Task<Result<bool, string>> SendRewardNotificationsAsync(RewardEventDto rewardEvent, CancellationToken cancellationToken = default)
    {
        try
        {
            if (rewardEvent == null)
                return Result<bool, string>.Failure("Reward event cannot be null");

            // Get webhooks for the event type
            var webhooksResult = await GetWebhooksByEventTypeAsync(rewardEvent.EventType, cancellationToken);
            if (!webhooksResult.IsSuccess)
                return Result<bool, string>.Failure($"Failed to get webhooks: {webhooksResult.Error}");

            var webhooks = webhooksResult.Value?.Where(w => w.IsActive) ?? new List<WebhookDto>();
            if (!webhooks.Any())
                return Result<bool, string>.Success(true); // No webhooks to notify

            var tasks = webhooks.Select(webhook => SendWebhookNotificationAsync(webhook, rewardEvent, cancellationToken));
            var results = await Task.WhenAll(tasks);

            var successCount = results.Count(r => r.IsSuccess);
            var totalCount = results.Length;

            if (successCount == totalCount)
                return Result<bool, string>.Success(true);

            return Result<bool, string>.Failure($"Only {successCount} of {totalCount} webhook notifications succeeded");
        }
        catch (Exception ex)
        {
            return Result<bool, string>.Failure($"Failed to send reward notifications: {ex.Message}");
        }
    }

    public async Task<Result<WebhookTestResultDto, string>> TestWebhookAsync(string webhookId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(webhookId))
                return Result<WebhookTestResultDto, string>.Failure("Webhook ID cannot be empty");

            var webhookResult = await GetWebhookByIdAsync(webhookId, cancellationToken);
            if (!webhookResult.IsSuccess)
                return Result<WebhookTestResultDto, string>.Failure($"Webhook not found: {webhookResult.Error}");

            var webhook = webhookResult.Value!;

            // Create test payload
            var testEvent = new RewardEventDto
            {
                EventId = Guid.NewGuid().ToString(),
                EventType = "test",
                UserId = "test-user",
                RewardType = "points",
                RewardId = "test-reward",
                RewardName = "Test Reward",
                PointsAmount = 100,
                PointCategory = "xp",
                Timestamp = DateTime.UtcNow,
                Metadata = new Dictionary<string, object> { { "test", true } }
            };

            var result = await SendWebhookNotificationAsync(webhook, testEvent, cancellationToken);

            return Result<WebhookTestResultDto, string>.Success(new WebhookTestResultDto
            {
                Success = result.IsSuccess,
                StatusCode = result.IsSuccess ? 200 : 500,
                ResponseBody = result.IsSuccess ? "OK" : result.Error,
                ErrorMessage = result.IsSuccess ? null : result.Error,
                ResponseTime = TimeSpan.FromMilliseconds(100), // TODO: Measure actual response time
                TestedAt = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            return Result<WebhookTestResultDto, string>.Failure($"Failed to test webhook: {ex.Message}");
        }
    }

    private async Task<Result<bool, string>> SendWebhookNotificationAsync(WebhookDto webhook, RewardEventDto rewardEvent, CancellationToken cancellationToken)
    {
        try
        {
            var json = JsonSerializer.Serialize(rewardEvent, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Add webhook signature if secret is provided
            if (!string.IsNullOrWhiteSpace(webhook.Secret))
            {
                var signature = GenerateSignature(json, webhook.Secret);
                content.Headers.Add("X-Webhook-Signature", signature);
            }

            _httpClient.Timeout = TimeSpan.FromSeconds(webhook.TimeoutSeconds);

            var response = await _httpClient.PostAsync(webhook.Url, content, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                // TODO: Update webhook last triggered timestamp
                return Result<bool, string>.Success(true);
            }

            return Result<bool, string>.Failure($"Webhook returned status code: {response.StatusCode}");
        }
        catch (HttpRequestException ex)
        {
            return Result<bool, string>.Failure($"HTTP request failed: {ex.Message}");
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            return Result<bool, string>.Failure("Webhook request timed out");
        }
        catch (Exception ex)
        {
            return Result<bool, string>.Failure($"Webhook notification failed: {ex.Message}");
        }
    }

    private static string GenerateSignature(string payload, string secret)
    {
        using var hmac = new System.Security.Cryptography.HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
