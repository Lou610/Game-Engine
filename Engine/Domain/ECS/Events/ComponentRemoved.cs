using Engine.Domain.ECS;
using Engine.Domain.ECS.ValueObjects;

namespace Engine.Domain.ECS.Events;

/// <summary>
/// Domain event for component removal
/// </summary>
public record ComponentRemoved(EntityId EntityId, Component Component);

