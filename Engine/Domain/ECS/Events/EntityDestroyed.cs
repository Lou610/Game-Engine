using Engine.Domain.ECS.ValueObjects;

namespace Engine.Domain.ECS.Events;

/// <summary>
/// Domain event for entity destruction
/// </summary>
public record EntityDestroyed(EntityId EntityId);

