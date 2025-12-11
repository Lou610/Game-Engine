using System.IO;
using Engine.Domain.Resources;
using Engine.Domain.Resources.ValueObjects;

namespace Engine.Infrastructure.Resources;

/// <summary>
/// Asset loading implementation
/// </summary>
public class AssetLoader
{
    public Asset? LoadFromFile(AssetPath path, AssetType type)
    {
        if (!File.Exists(path.Value))
        {
            return null;
        }

        var asset = new Asset
        {
            Id = AssetGuid.NewGuid(),
            Path = path,
            Type = type,
            Name = Path.GetFileName(path.Value)
        };

        return asset;
    }
}

