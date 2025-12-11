using Engine.Domain.ECS.ValueObjects;

namespace Engine.Domain.ECS;

/// <summary>
/// Base class for all components
/// </summary>
public abstract class Component
{
    public EntityId EntityId { get; internal set; }
}

