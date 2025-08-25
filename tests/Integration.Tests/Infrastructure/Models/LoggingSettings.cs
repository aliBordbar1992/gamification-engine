namespace GamificationEngine.Integration.Tests.Infrastructure.Models;

/// <summary>
/// Logging configuration settings
/// </summary>
public class LoggingSettings
{
    public string DefaultLevel { get; set; } = "Information";
    public string MicrosoftAspNetCoreLevel { get; set; } = "Warning";
    public string MicrosoftEntityFrameworkLevel { get; set; } = "Information";
    public bool EnableSensitiveDataLogging { get; set; } = false;
    public bool EnableStructuredLogging { get; set; } = true;
}