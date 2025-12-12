using System;

namespace Engine.Domain.ECS.Interfaces;

/// <summary>
/// Publisher interface for domain events
/// </summary>
public interface IDomainEventPublisher
{
    /// <summary>
    /// Publish a domain event to all subscribers
    /// </summary>
    void Publish<T>(T domainEvent) where T : IDomainEvent;
    
    /// <summary>
    /// Subscribe to a specific domain event type
    /// </summary>
    void Subscribe<T>(Action<T> handler) where T : IDomainEvent;
    
    /// <summary>
    /// Unsubscribe from a specific domain event type
    /// </summary>
    void Unsubscribe<T>(Action<T> handler) where T : IDomainEvent;
    
    /// <summary>
    /// Clear all subscriptions
    /// </summary>
    void Clear();
}