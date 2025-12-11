using Engine.Domain.Resources.ValueObjects;

namespace Engine.Domain.Resources;

/// <summary>
/// Aggregate root representing a game asset
/// </summary>
public class Asset
{
    public AssetGuid Id { get; set; }
    public AssetPath Path { get; set; }
    public AssetType Type { get; set; }
    public string Name { get; set; } = string.Empty;
}

