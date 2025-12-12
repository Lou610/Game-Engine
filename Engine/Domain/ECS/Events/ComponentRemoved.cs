using Engine.Domain.ECS;
using Engine.Domain.ECS.ValueObjects;

namespace Engine.Domain.ECS.Events;

/// <summary>
/// Domain event fired when a component is removed from an entity
/// </summary>
public sealed class ComponentRemoved : DomainEventBase
{
    public EntityId EntityId { get; }
    public ComponentType ComponentType { get; }
    public Component Component { get; }
    
    public ComponentRemoved(EntityId entityId, ComponentType componentType, Component component)
    {
        EntityId = entityId;
        ComponentType = componentType;
        Component = component;
    }
}

