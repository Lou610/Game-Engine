using Engine.Domain.Core;

namespace Engine.Domain.Core.Services;

/// <summary>
/// Domain service for configuration management
/// </summary>
public class ConfigurationService
{
    private ApplicationConfig _config;

    public ConfigurationService(ApplicationConfig config)
    {
        _config = config;
    }

    public ApplicationConfig GetConfig() => _config;

    public void UpdateConfig(ApplicationConfig newConfig)
    {
        _config = newConfig;
    }
}

