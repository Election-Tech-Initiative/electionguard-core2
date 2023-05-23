namespace ElectionGuard.UI.Services;

public class ConfigurationService : IConfigurationService
{
    public string GetVersion() => $"v{VersionTracking.CurrentVersion}";
}
