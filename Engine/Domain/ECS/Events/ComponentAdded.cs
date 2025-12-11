using Engine.Domain.ECS;
using Engine.Domain.ECS.ValueObjects;

namespace Engine.Domain.ECS.Events;

/// <summary>
/// Domain event for component addition
/// </summary>
public record ComponentAdded(EntityId EntityId, Component Component);

