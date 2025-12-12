using System;

namespace Engine.Domain.ECS.Interfaces;

/// <summary>
/// Marker interface for domain events
/// </summary>
public interface IDomainEvent
{
    /// <summary>
    /// When the event occurred
    /// </summary>
    DateTime OccurredAt { get; }
    
    /// <summary>
    /// Unique identifier for this event occurrence
    /// </summary>
    Guid EventId { get; }
}