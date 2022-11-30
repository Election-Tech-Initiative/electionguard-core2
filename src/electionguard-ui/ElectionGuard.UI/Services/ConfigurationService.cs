using ElectionGuard.UI.Lib.Services;

namespace ElectionGuard.UI.Services;

public class ConfigurationService : IConfigurationService
{
    public string GetVersion()
    {
        return $"v{VersionTracking.CurrentVersion}";
    }
}
