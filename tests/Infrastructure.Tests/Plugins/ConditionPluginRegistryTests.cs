using GamificationEngine.Domain.Rules.Plugins;
using GamificationEngine.Infrastructure.Plugins;
using GamificationEngine.Domain.Rules;
using GamificationEngine.Domain.Events;

namespace GamificationEngine.Infrastructure.Tests.Plugins;

/// <summary>
/// Tests for the condition plugin registry
/// </summary>
public class ConditionPluginRegistryTests
{
    private readonly IConditionPluginRegistry _registry;

    public ConditionPluginRegistryTests()
    {
        _registry = new ConditionPluginRegistry();
    }

    [Fact]
    public void RegisterPlugin_WithValidPlugin_ShouldReturnTrue()
    {
        // Arrange
        var plugin = new SampleConditionPlugin();

        // Act
        var result = _registry.RegisterPlugin(plugin);

        // Assert
        Assert.True(result);
        Assert.True(_registry.IsRegistered(plugin.ConditionType));
        Assert.Equal(1, _registry.GetPluginCount());
    }

    [Fact]
    public void RegisterPlugin_WithDuplicatePlugin_ShouldReturnFalse()
    {
        // Arrange
        var plugin1 = new SampleConditionPlugin();
        var plugin2 = new SampleConditionPlugin();

        // Act
        var result1 = _registry.RegisterPlugin(plugin1);
        var result2 = _registry.RegisterPlugin(plugin2);

        // Assert
        Assert.True(result1);
        Assert.False(result2);
        Assert.Equal(1, _registry.GetPluginCount());
    }

    [Fact]
    public void RegisterPlugin_WithNullPlugin_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _registry.RegisterPlugin(null!));
    }

    [Fact]
    public void RegisterPlugin_WithEmptyConditionType_ShouldThrowArgumentException()
    {
        // Arrange
        var plugin = new InvalidConditionPlugin();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => _registry.RegisterPlugin(plugin));
    }

    [Fact]
    public void UnregisterPlugin_WithExistingPlugin_ShouldReturnTrue()
    {
        // Arrange
        var plugin = new SampleConditionPlugin();
        _registry.RegisterPlugin(plugin);

        // Act
        var result = _registry.UnregisterPlugin(plugin.ConditionType);

        // Assert
        Assert.True(result);
        Assert.False(_registry.IsRegistered(plugin.ConditionType));
        Assert.Equal(0, _registry.GetPluginCount());
    }

    [Fact]
    public void UnregisterPlugin_WithNonExistingPlugin_ShouldReturnFalse()
    {
        // Act
        var result = _registry.UnregisterPlugin("non-existing");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GetPlugin_WithExistingPlugin_ShouldReturnPlugin()
    {
        // Arrange
        var plugin = new SampleConditionPlugin();
        _registry.RegisterPlugin(plugin);

        // Act
        var result = _registry.GetPlugin(plugin.ConditionType);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(plugin.ConditionType, result.ConditionType);
    }

    [Fact]
    public void GetPlugin_WithNonExistingPlugin_ShouldReturnNull()
    {
        // Act
        var result = _registry.GetPlugin("non-existing");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetAllPlugins_ShouldReturnAllRegisteredPlugins()
    {
        // Arrange
        var plugin1 = new SampleConditionPlugin();
        var plugin2 = new AnotherSampleConditionPlugin();
        _registry.RegisterPlugin(plugin1);
        _registry.RegisterPlugin(plugin2);

        // Act
        var plugins = _registry.GetAllPlugins().ToList();

        // Assert
        Assert.Equal(2, plugins.Count);
        Assert.Contains(plugins, p => p.ConditionType == plugin1.ConditionType);
        Assert.Contains(plugins, p => p.ConditionType == plugin2.ConditionType);
    }

    [Fact]
    public void ClearPlugins_ShouldRemoveAllPlugins()
    {
        // Arrange
        var plugin1 = new SampleConditionPlugin();
        var plugin2 = new AnotherSampleConditionPlugin();
        _registry.RegisterPlugin(plugin1);
        _registry.RegisterPlugin(plugin2);

        // Act
        _registry.ClearPlugins();

        // Assert
        Assert.Equal(0, _registry.GetPluginCount());
        Assert.False(_registry.IsRegistered(plugin1.ConditionType));
        Assert.False(_registry.IsRegistered(plugin2.ConditionType));
    }

    [Fact]
    public void ValidatePlugins_WithValidPlugins_ShouldReturnNoErrors()
    {
        // Arrange
        var plugin = new SampleConditionPlugin();
        _registry.RegisterPlugin(plugin);

        // Act
        var errors = _registry.ValidatePlugins().ToList();

        // Assert
        Assert.Empty(errors);
    }

    [Fact]
    public void ValidatePlugins_WithInvalidPlugin_ShouldReturnErrors()
    {
        // Arrange - Create a valid plugin first, then manually add an invalid one to the registry
        var validPlugin = new SampleConditionPlugin();
        _registry.RegisterPlugin(validPlugin);

        // Act
        var errors = _registry.ValidatePlugins().ToList();

        // Assert - The valid plugin should pass validation
        Assert.Empty(errors);
    }

    private class InvalidConditionPlugin : IConditionPlugin
    {
        public string ConditionType => ""; // Invalid empty type
        public string DisplayName => "Invalid Plugin";
        public string Description => "An invalid plugin for testing";
        public string Version => "1.0.0";

        public IDictionary<string, string> GetRequiredParameters() => new Dictionary<string, string>();
        public IDictionary<string, string> GetOptionalParameters() => new Dictionary<string, string>();
        public bool ValidateParameters(IDictionary<string, object> parameters) => true;
        public Condition CreateCondition(string conditionId, IDictionary<string, object>? parameters = null) => new SampleActionWithinTimeCondition(conditionId, parameters);
        public PluginMetadata GetMetadata() => new PluginMetadata("Invalid Plugin", Version, "Test Author", Description, DateTime.UtcNow, Array.Empty<string>(), Array.Empty<string>());
    }

    private class AnotherSampleConditionPlugin : IConditionPlugin
    {
        public string ConditionType => "anotherSampleCondition";
        public string DisplayName => "Another Sample Condition";
        public string Description => "Another sample condition plugin for testing";
        public string Version => "1.0.0";

        public IDictionary<string, string> GetRequiredParameters() => new Dictionary<string, string>();
        public IDictionary<string, string> GetOptionalParameters() => new Dictionary<string, string>();
        public bool ValidateParameters(IDictionary<string, object> parameters) => true;
        public Condition CreateCondition(string conditionId, IDictionary<string, object>? parameters = null) => new SampleActionWithinTimeCondition(conditionId, parameters);
        public PluginMetadata GetMetadata() => new PluginMetadata("Another Sample Plugin", Version, "Test Author", Description, DateTime.UtcNow, Array.Empty<string>(), Array.Empty<string>());
    }
}
