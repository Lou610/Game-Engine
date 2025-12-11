using System.Collections.Generic;
using Engine.Domain.Resources;
using Engine.Domain.Resources.ValueObjects;

namespace Engine.Infrastructure.Resources;

/// <summary>
/// Repository for asset persistence
/// </summary>
public class AssetRepository
{
    private readonly Dictionary<AssetGuid, Asset> _assets = new();

    public void Save(Asset asset)
    {
        _assets[asset.Id] = asset;
    }

    public Asset? Load(AssetGuid id)
    {
        return _assets.TryGetValue(id, out var asset) ? asset : null;
    }

    public void Delete(AssetGuid id)
    {
        _assets.Remove(id);
    }

    public IEnumerable<Asset> GetAll()
    {
        return _assets.Values;
    }
}

