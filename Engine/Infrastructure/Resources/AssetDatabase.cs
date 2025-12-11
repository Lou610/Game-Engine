using System.Collections.Generic;
using Engine.Domain.Resources;
using Engine.Domain.Resources.ValueObjects;

namespace Engine.Infrastructure.Resources;

/// <summary>
/// Asset metadata database
/// </summary>
public class AssetDatabase
{
    private readonly Dictionary<AssetGuid, AssetMetadata> _metadata = new();

    public void AddMetadata(AssetMetadata metadata)
    {
        _metadata[metadata.AssetId] = metadata;
    }

    public AssetMetadata? GetMetadata(AssetGuid assetId)
    {
        return _metadata.TryGetValue(assetId, out var metadata) ? metadata : null;
    }
}

