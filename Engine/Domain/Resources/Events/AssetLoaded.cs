using Engine.Domain.Resources.ValueObjects;

namespace Engine.Domain.Resources.Events;

/// <summary>
/// Domain event for asset loaded
/// </summary>
public record AssetLoaded(AssetGuid AssetId, AssetType AssetType);

