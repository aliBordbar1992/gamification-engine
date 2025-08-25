using GamificationEngine.Domain.Events;
using GamificationEngine.Domain.Users;

namespace GamificationEngine.Integration.Tests.Infrastructure.Models;

/// <summary>
/// Represents a collection of test data for a specific scenario
/// </summary>
public class TestDataFixture
{
    /// <summary>
    /// Name of the fixture
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Events in the fixture
    /// </summary>
    public List<Event> Events { get; set; } = new();

    /// <summary>
    /// User states in the fixture
    /// </summary>
    public List<UserState> UserStates { get; set; } = new();

    /// <summary>
    /// Metadata for the fixture
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();
}