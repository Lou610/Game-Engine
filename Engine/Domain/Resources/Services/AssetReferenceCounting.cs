using System.Collections.Generic;
using Engine.Domain.Resources;
using Engine.Domain.Resources.ValueObjects;

namespace Engine.Domain.Resources.Services;

/// <summary>
/// Reference counting domain logic
/// </summary>
public class AssetReferenceCounting
{
    private readonly Dictionary<AssetGuid, int> _referenceCounts = new();

    public void AddReference(AssetGuid assetId)
    {
        if (_referenceCounts.TryGetValue(assetId, out var count))
        {
            _referenceCounts[assetId] = count + 1;
        }
        else
        {
            _referenceCounts[assetId] = 1;
        }
    }

    public void RemoveReference(AssetGuid assetId)
    {
        if (_referenceCounts.TryGetValue(assetId, out var count))
        {
            if (count > 1)
            {
                _referenceCounts[assetId] = count - 1;
            }
            else
            {
                _referenceCounts.Remove(assetId);
            }
        }
    }

    public int GetReferenceCount(AssetGuid assetId)
    {
        return _referenceCounts.TryGetValue(assetId, out var count) ? count : 0;
    }
}

