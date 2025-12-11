using System.Collections.Generic;
using Engine.Domain.Resources;
using Engine.Domain.Resources.ValueObjects;

namespace Engine.Infrastructure.Resources;

/// <summary>
/// Caching implementation
/// </summary>
public class AssetCache
{
    private readonly Dictionary<AssetGuid, Asset> _cache = new();

    public void Add(Asset asset)
    {
        _cache[asset.Id] = asset;
    }

    public Asset? Get(AssetGuid id)
    {
        return _cache.TryGetValue(id, out var asset) ? asset : null;
    }

    public void Remove(AssetGuid id)
    {
        _cache.Remove(id);
    }

    public void Clear()
    {
        _cache.Clear();
    }
}

