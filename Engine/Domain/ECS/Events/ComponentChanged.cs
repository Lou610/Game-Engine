using Engine.Domain.ECS;
using Engine.Domain.ECS.ValueObjects;

namespace Engine.Domain.ECS.Events;

/// <summary>
/// Domain event fired when a component's data is modified
/// </summary>
public sealed class ComponentChanged : DomainEventBase
{
    public EntityId EntityId { get; }
    public ComponentType ComponentType { get; }
    public Component Component { get; }
    public string? PropertyName { get; }
    
    public ComponentChanged(EntityId entityId, ComponentType componentType, Component component, string? propertyName = null)
    {
        EntityId = entityId;
        ComponentType = componentType;
        Component = component;
        PropertyName = propertyName;
    }
}