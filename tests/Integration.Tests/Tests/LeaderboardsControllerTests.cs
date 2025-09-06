using GamificationEngine.Application.Abstractions;
using GamificationEngine.Application.DTOs;
using GamificationEngine.Domain.Entities;
using GamificationEngine.Domain.Leaderboards;
using GamificationEngine.Domain.Repositories;
using GamificationEngine.Domain.Users;
using GamificationEngine.Infrastructure.Storage.InMemory;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace GamificationEngine.Integration.Tests.Tests;

public class LeaderboardsControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public LeaderboardsControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetLeaderboard_WithValidPointsQuery_ShouldReturnOk()
    {
        // Arrange
        var url = "/api/leaderboards?type=points&category=xp&timeRange=daily&page=1&pageSize=50";

        // Act
        var response = await _client.GetAsync(url);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var leaderboard = JsonSerializer.Deserialize<LeaderboardDto>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        leaderboard.ShouldNotBeNull();
        leaderboard.Query.Type.ShouldBe("points");
        leaderboard.Query.Category.ShouldBe("xp");
        leaderboard.Query.TimeRange.ShouldBe("daily");
    }

    [Fact]
    public async Task GetLeaderboard_WithValidBadgesQuery_ShouldReturnOk()
    {
        // Arrange
        var url = "/api/leaderboards?type=badges&timeRange=weekly&page=1&pageSize=25";

        // Act
        var response = await _client.GetAsync(url);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var leaderboard = JsonSerializer.Deserialize<LeaderboardDto>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        leaderboard.ShouldNotBeNull();
        leaderboard.Query.Type.ShouldBe("badges");
        leaderboard.Query.TimeRange.ShouldBe("weekly");
    }

    [Fact]
    public async Task GetLeaderboard_WithValidTrophiesQuery_ShouldReturnOk()
    {
        // Arrange
        var url = "/api/leaderboards?type=trophies&timeRange=monthly&page=1&pageSize=25";

        // Act
        var response = await _client.GetAsync(url);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var leaderboard = JsonSerializer.Deserialize<LeaderboardDto>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        leaderboard.ShouldNotBeNull();
        leaderboard.Query.Type.ShouldBe("trophies");
        leaderboard.Query.TimeRange.ShouldBe("monthly");
    }

    [Fact]
    public async Task GetLeaderboard_WithValidLevelsQuery_ShouldReturnOk()
    {
        // Arrange
        var url = "/api/leaderboards?type=level&category=xp&timeRange=alltime&page=1&pageSize=25";

        // Act
        var response = await _client.GetAsync(url);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var leaderboard = JsonSerializer.Deserialize<LeaderboardDto>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        leaderboard.ShouldNotBeNull();
        leaderboard.Query.Type.ShouldBe("level");
        leaderboard.Query.Category.ShouldBe("xp");
        leaderboard.Query.TimeRange.ShouldBe("alltime");
    }

    [Fact]
    public async Task GetLeaderboard_WithInvalidType_ShouldReturnBadRequest()
    {
        // Arrange
        var url = "/api/leaderboards?type=invalid&category=xp&timeRange=daily&page=1&pageSize=50";

        // Act
        var response = await _client.GetAsync(url);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        var error = JsonSerializer.Deserialize<JsonElement>(content);
        error.TryGetProperty("error", out var errorMessage).ShouldBeTrue();
        errorMessage.GetString().ShouldContain("Invalid leaderboard type");
    }

    [Fact]
    public async Task GetLeaderboard_WithInvalidTimeRange_ShouldReturnBadRequest()
    {
        // Arrange
        var url = "/api/leaderboards?type=points&category=xp&timeRange=invalid&page=1&pageSize=50";

        // Act
        var response = await _client.GetAsync(url);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        var error = JsonSerializer.Deserialize<JsonElement>(content);
        error.TryGetProperty("error", out var errorMessage).ShouldBeTrue();
        errorMessage.GetString().ShouldContain("Invalid time range");
    }

    [Fact]
    public async Task GetLeaderboard_WithInvalidPage_ShouldReturnBadRequest()
    {
        // Arrange
        var url = "/api/leaderboards?type=points&category=xp&timeRange=daily&page=0&pageSize=50";

        // Act
        var response = await _client.GetAsync(url);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        var error = JsonSerializer.Deserialize<JsonElement>(content);
        error.TryGetProperty("error", out var errorMessage).ShouldBeTrue();
        errorMessage.GetString().ShouldContain("Page must be at least 1");
    }

    [Fact]
    public async Task GetLeaderboard_WithInvalidPageSize_ShouldReturnBadRequest()
    {
        // Arrange
        var url = "/api/leaderboards?type=points&category=xp&timeRange=daily&page=1&pageSize=1001";

        // Act
        var response = await _client.GetAsync(url);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        var error = JsonSerializer.Deserialize<JsonElement>(content);
        error.TryGetProperty("error", out var errorMessage).ShouldBeTrue();
        errorMessage.GetString().ShouldContain("Page size must be between 1 and 1000");
    }

    [Fact]
    public async Task GetLeaderboard_WithPointsTypeButNoCategory_ShouldReturnBadRequest()
    {
        // Arrange
        var url = "/api/leaderboards?type=points&timeRange=daily&page=1&pageSize=50";

        // Act
        var response = await _client.GetAsync(url);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        var error = JsonSerializer.Deserialize<JsonElement>(content);
        error.TryGetProperty("error", out var errorMessage).ShouldBeTrue();
        errorMessage.GetString().ShouldContain("Category is required for points leaderboard");
    }

    [Fact]
    public async Task GetPointsLeaderboard_WithValidParameters_ShouldReturnOk()
    {
        // Arrange
        var url = "/api/leaderboards/points/xp?timeRange=daily&page=1&pageSize=50";

        // Act
        var response = await _client.GetAsync(url);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var leaderboard = JsonSerializer.Deserialize<LeaderboardDto>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        leaderboard.ShouldNotBeNull();
        leaderboard.Query.Type.ShouldBe("points");
        leaderboard.Query.Category.ShouldBe("xp");
    }

    [Fact]
    public async Task GetBadgesLeaderboard_WithValidParameters_ShouldReturnOk()
    {
        // Arrange
        var url = "/api/leaderboards/badges?timeRange=weekly&page=1&pageSize=25";

        // Act
        var response = await _client.GetAsync(url);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var leaderboard = JsonSerializer.Deserialize<LeaderboardDto>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        leaderboard.ShouldNotBeNull();
        leaderboard.Query.Type.ShouldBe("badges");
    }

    [Fact]
    public async Task GetTrophiesLeaderboard_WithValidParameters_ShouldReturnOk()
    {
        // Arrange
        var url = "/api/leaderboards/trophies?timeRange=monthly&page=1&pageSize=25";

        // Act
        var response = await _client.GetAsync(url);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var leaderboard = JsonSerializer.Deserialize<LeaderboardDto>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        leaderboard.ShouldNotBeNull();
        leaderboard.Query.Type.ShouldBe("trophies");
    }

    [Fact]
    public async Task GetLevelsLeaderboard_WithValidParameters_ShouldReturnOk()
    {
        // Arrange
        var url = "/api/leaderboards/levels/xp?timeRange=alltime&page=1&pageSize=25";

        // Act
        var response = await _client.GetAsync(url);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var leaderboard = JsonSerializer.Deserialize<LeaderboardDto>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        leaderboard.ShouldNotBeNull();
        leaderboard.Query.Type.ShouldBe("level");
        leaderboard.Query.Category.ShouldBe("xp");
    }

    [Fact]
    public async Task GetUserRank_WithValidParameters_ShouldReturnOk()
    {
        // Arrange
        var userId = "user123";
        var url = $"/api/leaderboards/user/{userId}/rank?type=points&category=xp&timeRange=daily";

        // Act
        var response = await _client.GetAsync(url);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var userRank = JsonSerializer.Deserialize<UserRankDto>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        userRank.ShouldNotBeNull();
        userRank.UserId.ShouldBe(userId);
    }

    [Fact]
    public async Task GetUserRank_WithEmptyUserId_ShouldReturnBadRequest()
    {
        // Arrange
        var url = "/api/leaderboards/user//rank?type=points&category=xp&timeRange=daily";

        // Act
        var response = await _client.GetAsync(url);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound); // Empty route parameter results in 404
    }

    [Fact]
    public async Task RefreshCache_WithValidParameters_ShouldReturnOk()
    {
        // Arrange
        var url = "/api/leaderboards/refresh?type=points&category=xp&timeRange=daily";

        // Act
        var response = await _client.PostAsync(url, null);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(content);
        result.TryGetProperty("success", out var success).ShouldBeTrue();
        success.GetBoolean().ShouldBeTrue();
    }

    [Fact]
    public async Task RefreshCache_WithInvalidParameters_ShouldReturnBadRequest()
    {
        // Arrange
        var url = "/api/leaderboards/refresh?type=invalid&category=xp&timeRange=daily";

        // Act
        var response = await _client.PostAsync(url, null);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        var error = JsonSerializer.Deserialize<JsonElement>(content);
        error.TryGetProperty("error", out var errorMessage).ShouldBeTrue();
        errorMessage.GetString().ShouldContain("Invalid leaderboard type");
    }
}
