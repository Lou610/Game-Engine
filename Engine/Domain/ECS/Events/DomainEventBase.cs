using System;
using Engine.Domain.ECS.Interfaces;

namespace Engine.Domain.ECS.Events;

/// <summary>
/// Base class for domain events providing common properties
/// </summary>
public abstract class DomainEventBase : IDomainEvent
{
    public DateTime OccurredAt { get; }
    public Guid EventId { get; }
    
    protected DomainEventBase()
    {
        OccurredAt = DateTime.UtcNow;
        EventId = Guid.NewGuid();
    }
}