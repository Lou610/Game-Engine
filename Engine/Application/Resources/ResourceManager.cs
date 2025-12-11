using Engine.Domain.Resources;
using Engine.Domain.Resources.Services;
using Engine.Domain.Resources.ValueObjects;
using Engine.Infrastructure.Logging;

namespace Engine.Application.Resources;

/// <summary>
/// Application service for resource orchestration
/// </summary>
public class ResourceManager
{
    private readonly AssetLifecycle _lifecycle;
    private readonly AssetReferenceCounting _referenceCounting;
    private readonly Logger _logger;

    public ResourceManager(AssetLifecycle lifecycle, AssetReferenceCounting referenceCounting, Logger logger)
    {
        _lifecycle = lifecycle;
        _referenceCounting = referenceCounting;
        _logger = logger;
    }

    public void LoadAsset(Asset asset)
    {
        _lifecycle.LoadAsset(asset);
        _referenceCounting.AddReference(asset.Id);
        _logger.Debug($"Asset loaded: {asset.Name}");
    }

    public void UnloadAsset(Asset asset)
    {
        _referenceCounting.RemoveReference(asset.Id);
        if (_referenceCounting.GetReferenceCount(asset.Id) == 0)
        {
            _lifecycle.UnloadAsset(asset);
            _logger.Debug($"Asset unloaded: {asset.Name}");
        }
    }
}

