using System;
using System.Collections.Generic;
using Engine.Domain.ECS.Interfaces;

namespace Engine.Application.ECS;

/// <summary>
/// Implementation of domain event publisher for ECS events
/// </summary>
public class DomainEventPublisher : IDomainEventPublisher
{
    private readonly Dictionary<Type, List<Delegate>> _subscriptions;

    public DomainEventPublisher()
    {
        _subscriptions = new Dictionary<Type, List<Delegate>>();
    }

    /// <summary>
    /// Publish a domain event to all subscribers
    /// </summary>
    public void Publish<T>(T domainEvent) where T : IDomainEvent
    {
        var eventType = typeof(T);
        
        if (!_subscriptions.TryGetValue(eventType, out var handlers))
            return;

        // Create a copy to avoid modification during enumeration
        var handlersCopy = new List<Delegate>(handlers);
        
        foreach (var handler in handlersCopy)
        {
            try
            {
                ((Action<T>)handler)(domainEvent);
            }
            catch (Exception ex)
            {
                // Log the exception but don't stop other handlers
                // In a real implementation, you'd use proper logging
                Console.WriteLine($"Error in event handler for {eventType.Name}: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Subscribe to a specific domain event type
    /// </summary>
    public void Subscribe<T>(Action<T> handler) where T : IDomainEvent
    {
        if (handler == null)
            throw new ArgumentNullException(nameof(handler));

        var eventType = typeof(T);
        
        if (!_subscriptions.TryGetValue(eventType, out var handlers))
        {
            handlers = new List<Delegate>();
            _subscriptions[eventType] = handlers;
        }

        handlers.Add(handler);
    }

    /// <summary>
    /// Unsubscribe from a specific domain event type
    /// </summary>
    public void Unsubscribe<T>(Action<T> handler) where T : IDomainEvent
    {
        if (handler == null)
            return;

        var eventType = typeof(T);
        
        if (_subscriptions.TryGetValue(eventType, out var handlers))
        {
            handlers.Remove(handler);
            
            // Clean up empty subscription lists
            if (handlers.Count == 0)
            {
                _subscriptions.Remove(eventType);
            }
        }
    }

    /// <summary>
    /// Clear all subscriptions
    /// </summary>
    public void Clear()
    {
        _subscriptions.Clear();
    }

    /// <summary>
    /// Get the number of subscribers for a specific event type
    /// </summary>
    public int GetSubscriberCount<T>() where T : IDomainEvent
    {
        var eventType = typeof(T);
        return _subscriptions.TryGetValue(eventType, out var handlers) ? handlers.Count : 0;
    }

    /// <summary>
    /// Get all event types that have subscribers
    /// </summary>
    public IEnumerable<Type> GetSubscribedEventTypes()
    {
        return _subscriptions.Keys;
    }
}