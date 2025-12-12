using Engine.Domain.ECS.ValueObjects;

namespace Engine.Domain.ECS.Events;

/// <summary>
/// Domain event fired when an entity is destroyed
/// </summary>
public sealed class EntityDestroyed : DomainEventBase
{
    public EntityId EntityId { get; }
    public string EntityName { get; }
    
    public EntityDestroyed(EntityId entityId, string entityName)
    {
        EntityId = entityId;
        EntityName = entityName;
    }
}

