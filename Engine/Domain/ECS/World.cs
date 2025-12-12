using System;
using System.Collections.Generic;
using System.Linq;
using Engine.Domain.ECS.ValueObjects;
using Engine.Domain.ECS.Interfaces;
using Engine.Domain.ECS.Events;
using Engine.Infrastructure.ECS;

namespace Engine.Domain.ECS;

/// <summary>
/// Aggregate root managing entities and components
/// </summary>
public class World : IDisposable
{
    private readonly Dictionary<EntityId, Entity> _entities = new();
    private readonly ComponentStorage _componentStorage;
    private readonly IDomainEventPublisher? _eventPublisher;
    private ulong _nextEntityId = 1;
    private bool _disposed;

    public World(IDomainEventPublisher? eventPublisher = null)
    {
        _componentStorage = new ComponentStorage();
        _eventPublisher = eventPublisher;
    }

    /// <summary>
    /// Create a new entity in the world
    /// </summary>
    public Entity CreateEntity(string name = "")
    {
        ThrowIfDisposed();
        
        var id = new EntityId(_nextEntityId++);
        var entity = new Entity(id, name);
        _entities[id] = entity;
        
        _eventPublisher?.Publish(new EntityCreated(id, entity.Name));
        
        return entity;
    }

    /// <summary>
    /// Destroy an entity and all its components
    /// </summary>
    public void DestroyEntity(EntityId id)
    {
        ThrowIfDisposed();
        
        if (!_entities.TryGetValue(id, out var entity))
            return;
            
        // Remove all components
        _componentStorage.RemoveAllComponents(id);
        
        // Remove entity
        _entities.Remove(id);
        
        _eventPublisher?.Publish(new EntityDestroyed(id, entity.Name));
    }

    /// <summary>
    /// Get an entity by ID
    /// </summary>
    public Entity? GetEntity(EntityId id)
    {
        ThrowIfDisposed();
        return _entities.TryGetValue(id, out var entity) ? entity : null;
    }

    /// <summary>
    /// Get all entities in the world
    /// </summary>
    public IEnumerable<Entity> GetAllEntities()
    {
        ThrowIfDisposed();
        return _entities.Values;
    }

    /// <summary>
    /// Add a component to an entity
    /// </summary>
    public T AddComponent<T>(EntityId entityId, T component) where T : Component
    {
        ThrowIfDisposed();
        
        if (!_entities.ContainsKey(entityId))
            throw new InvalidOperationException($"Entity {entityId} does not exist");
            
        component.EntityId = entityId;
        _componentStorage.AddComponent(entityId, component);
        
        var componentType = new ComponentType(typeof(T));
        _eventPublisher?.Publish(new ComponentAdded(entityId, componentType, component));
        
        return component;
    }

    /// <summary>
    /// Add a component to an entity (creates component instance)
    /// </summary>
    public T AddComponent<T>(EntityId entityId) where T : Component, new()
    {
        return AddComponent(entityId, new T());
    }

    /// <summary>
    /// Get a component from an entity
    /// </summary>
    public T? GetComponent<T>(EntityId entityId) where T : Component
    {
        ThrowIfDisposed();
        return _componentStorage.GetComponent<T>(entityId);
    }

    /// <summary>
    /// Check if an entity has a specific component
    /// </summary>
    public bool HasComponent<T>(EntityId entityId) where T : Component
    {
        ThrowIfDisposed();
        return _componentStorage.HasComponent<T>(entityId);
    }

    /// <summary>
    /// Remove a component from an entity
    /// </summary>
    public void RemoveComponent<T>(EntityId entityId) where T : Component
    {
        ThrowIfDisposed();
        
        // Get the component before removing it for the event
        var component = _componentStorage.GetComponent<T>(entityId);
        if (component != null && _componentStorage.RemoveComponent<T>(entityId))
        {
            var componentType = new ComponentType(typeof(T));
            _eventPublisher?.Publish(new ComponentRemoved(entityId, componentType, component));
        }
    }

    /// <summary>
    /// Query entities that have a specific component
    /// </summary>
    public IEnumerable<Entity> Query<T>() where T : Component
    {
        ThrowIfDisposed();
        
        var entityIds = _componentStorage.GetEntitiesWithComponent<T>();
        return entityIds.Select(id => _entities[id]).Where(e => e != null);
    }

    /// <summary>
    /// Query entities that have two specific components
    /// </summary>
    public IEnumerable<Entity> Query<T1, T2>() 
        where T1 : Component 
        where T2 : Component
    {
        ThrowIfDisposed();
        
        var entityIds1 = _componentStorage.GetEntitiesWithComponent<T1>().ToHashSet();
        var entityIds2 = _componentStorage.GetEntitiesWithComponent<T2>();
        
        return entityIds2
            .Where(id => entityIds1.Contains(id))
            .Select(id => _entities[id])
            .Where(e => e != null);
    }

    /// <summary>
    /// Query entities that have three specific components
    /// </summary>
    public IEnumerable<Entity> Query<T1, T2, T3>() 
        where T1 : Component 
        where T2 : Component 
        where T3 : Component
    {
        ThrowIfDisposed();
        
        var entityIds1 = _componentStorage.GetEntitiesWithComponent<T1>().ToHashSet();
        var entityIds2 = _componentStorage.GetEntitiesWithComponent<T2>().ToHashSet();
        var entityIds3 = _componentStorage.GetEntitiesWithComponent<T3>();
        
        return entityIds3
            .Where(id => entityIds1.Contains(id) && entityIds2.Contains(id))
            .Select(id => _entities[id])
            .Where(e => e != null);
    }

    /// <summary>
    /// Get all components of a specific type
    /// </summary>
    public IEnumerable<T> GetAllComponents<T>() where T : Component
    {
        ThrowIfDisposed();
        return _componentStorage.GetAllComponents<T>();
    }

    /// <summary>
    /// Get all entities with their component of a specific type
    /// </summary>
    public IEnumerable<(Entity Entity, T Component)> GetEntitiesWithComponent<T>() where T : Component
    {
        ThrowIfDisposed();
        
        return _componentStorage.GetEntitiesWithComponent<T>()
            .Where(id => _entities.ContainsKey(id))
            .Select(id => (_entities[id], _componentStorage.GetComponent<T>(id)!));
    }

    /// <summary>
    /// Get the total number of entities
    /// </summary>
    public int EntityCount => _entities.Count;

    /// <summary>
    /// Clear all entities and components
    /// </summary>
    public void Clear()
    {
        ThrowIfDisposed();
        
        var entityIds = _entities.Keys.ToList();
        foreach (var entityId in entityIds)
        {
            DestroyEntity(entityId);
        }
    }

    /// <summary>
    /// Events for entity lifecycle
    /// </summary>
    public event Action<EntityId, Component>? ComponentAdded;
    public event Action<EntityId, Component>? ComponentRemoved;
    public event Action<EntityId>? EntityDestroyed;

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(World));
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            Clear();
            _componentStorage?.Dispose();
            _disposed = true;
        }
    }
}

