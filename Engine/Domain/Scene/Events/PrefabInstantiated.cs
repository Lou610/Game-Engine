using Engine.Domain.ECS.ValueObjects;
using Engine.Domain.Scene.ValueObjects;

namespace Engine.Domain.Scene.Events;

/// <summary>
/// Domain event for prefab instantiated
/// </summary>
public record PrefabInstantiated(PrefabId PrefabId, EntityId EntityId);

