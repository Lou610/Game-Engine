using Engine.Domain.Resources.ValueObjects;

namespace Engine.Domain.Resources;

/// <summary>
/// Entity with asset information
/// </summary>
public class AssetMetadata
{
    public AssetGuid AssetId { get; set; }
    public long FileSize { get; set; }
    public System.DateTime LastModified { get; set; }
}

