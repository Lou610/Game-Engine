using Engine.Domain.ECS.ValueObjects;

namespace Engine.Domain.ECS.Events;

/// <summary>
/// Domain event for entity creation
/// </summary>
public record EntityCreated(EntityId EntityId, string Name);

