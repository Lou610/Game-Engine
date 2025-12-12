using Engine.Domain.ECS.ValueObjects;

namespace Engine.Domain.ECS.Events;

/// <summary>
/// Domain event fired when an entity is created
/// </summary>
public sealed class EntityCreated : DomainEventBase
{
    public EntityId EntityId { get; }
    public string EntityName { get; }
    
    public EntityCreated(EntityId entityId, string entityName)
    {
        EntityId = entityId;
        EntityName = entityName;
    }
}

