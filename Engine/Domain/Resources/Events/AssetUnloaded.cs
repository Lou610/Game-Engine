using Engine.Domain.Resources.ValueObjects;

namespace Engine.Domain.Resources.Events;

/// <summary>
/// Domain event for asset unloaded
/// </summary>
public record AssetUnloaded(AssetGuid AssetId);

