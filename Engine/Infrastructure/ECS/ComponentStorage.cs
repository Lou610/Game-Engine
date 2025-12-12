using System;
using System.Collections.Generic;
using System.Linq;
using Engine.Domain.ECS;
using Engine.Domain.ECS.ValueObjects;
using Engine.Domain.ECS.Queries;

namespace Engine.Infrastructure.ECS;

/// <summary>
/// Efficient component storage implementation using archetypes
/// Groups entities with identical component signatures for cache-friendly access
/// </summary>
public class ComponentStorage : IDisposable
{
    private readonly Dictionary<ComponentSignature, Archetype> _archetypes;
    private readonly Dictionary<EntityId, Archetype> _entityToArchetype;
    private readonly Dictionary<EntityId, HashSet<Type>> _entityComponents;
    private bool _disposed;

    public ComponentStorage()
    {
        _archetypes = new Dictionary<ComponentSignature, Archetype>();
        _entityToArchetype = new Dictionary<EntityId, Archetype>();
        _entityComponents = new Dictionary<EntityId, HashSet<Type>>();
    }

    /// <summary>
    /// Add a component to an entity, moving it to the appropriate archetype
    /// </summary>
    public void AddComponent<T>(EntityId entityId, T component) where T : Component
    {
        ThrowIfDisposed();
        
        var componentType = typeof(T);
        
        // Get current components for this entity
        if (!_entityComponents.TryGetValue(entityId, out var currentComponents))
        {
            currentComponents = new HashSet<Type>();
            _entityComponents[entityId] = currentComponents;
        }
        
        if (currentComponents.Contains(componentType))
        {
            // Component already exists, just update it
            UpdateExistingComponent(entityId, component);
            return;
        }
        
        // Add new component type
        currentComponents.Add(componentType);
        
        // Calculate new signature
        var newSignature = new ComponentSignature(currentComponents);
        
        // Get or create target archetype
        var targetArchetype = GetOrCreateArchetype(newSignature, currentComponents);
        
        // Move entity from old archetype to new archetype
        MoveEntityToArchetype(entityId, targetArchetype, component);
    }

    /// <summary>
    /// Get a component from an entity
    /// </summary>
    public T? GetComponent<T>(EntityId entityId) where T : Component
    {
        ThrowIfDisposed();
        
        if (!_entityToArchetype.TryGetValue(entityId, out var archetype))
            return null;
            
        return archetype.GetComponent<T>(entityId);
    }

    /// <summary>
    /// Check if an entity has a specific component
    /// </summary>
    public bool HasComponent<T>(EntityId entityId) where T : Component
    {
        ThrowIfDisposed();
        
        if (!_entityComponents.TryGetValue(entityId, out var components))
            return false;
            
        return components.Contains(typeof(T));
    }

    /// <summary>
    /// Remove a component from an entity
    /// </summary>
    public bool RemoveComponent<T>(EntityId entityId) where T : Component
    {
        ThrowIfDisposed();
        
        var componentType = typeof(T);
        
        if (!_entityComponents.TryGetValue(entityId, out var currentComponents))
            return false;
            
        if (!currentComponents.Remove(componentType))
            return false;
        
        if (currentComponents.Count == 0)
        {
            // Entity has no components left, remove it entirely
            RemoveEntityCompletely(entityId);
        }
        else
        {
            // Calculate new signature and move to appropriate archetype
            var newSignature = new ComponentSignature(currentComponents);
            var targetArchetype = GetOrCreateArchetype(newSignature, currentComponents);
            MoveEntityToArchetype(entityId, targetArchetype);
        }
        
        return true;
    }

    /// <summary>
    /// Remove all components from an entity
    /// </summary>
    public void RemoveAllComponents(EntityId entityId)
    {
        ThrowIfDisposed();
        RemoveEntityCompletely(entityId);
    }

    /// <summary>
    /// Get all entities that have a specific component
    /// </summary>
    public IEnumerable<EntityId> GetEntitiesWithComponent<T>() where T : Component
    {
        ThrowIfDisposed();
        
        var componentType = typeof(T);
        
        foreach (var archetype in _archetypes.Values)
        {
            if (archetype.Signature.Contains(componentType))
            {
                foreach (var entityId in archetype.Entities)
                {
                    yield return entityId;
                }
            }
        }
    }

    /// <summary>
    /// Get all components of a specific type
    /// </summary>
    public IEnumerable<T> GetAllComponents<T>() where T : Component
    {
        ThrowIfDisposed();
        
        var componentType = typeof(T);
        
        foreach (var archetype in _archetypes.Values)
        {
            if (archetype.Signature.Contains(componentType))
            {
                var componentArray = archetype.GetComponentArray<T>();
                for (int i = 0; i < archetype.EntityCount; i++)
                {
                    yield return componentArray[i];
                }
            }
        }
    }

    /// <summary>
    /// Get all archetypes (for advanced queries)
    /// </summary>
    public IEnumerable<Archetype> GetArchetypes()
    {
        ThrowIfDisposed();
        return _archetypes.Values;
    }

    /// <summary>
    /// Get archetype for a specific component signature
    /// </summary>
    public Archetype? GetArchetype(ComponentSignature signature)
    {
        ThrowIfDisposed();
        return _archetypes.TryGetValue(signature, out var archetype) ? archetype : null;
    }

    private void UpdateExistingComponent<T>(EntityId entityId, T component) where T : Component
    {
        if (_entityToArchetype.TryGetValue(entityId, out var archetype))
        {
            archetype.SetComponent(entityId, component);
        }
    }

    private Archetype GetOrCreateArchetype(ComponentSignature signature, IEnumerable<Type> componentTypes)
    {
        if (!_archetypes.TryGetValue(signature, out var archetype))
        {
            archetype = new Archetype(signature, componentTypes);
            _archetypes[signature] = archetype;
        }
        
        return archetype;
    }

    private void MoveEntityToArchetype(EntityId entityId, Archetype targetArchetype, Component? newComponent = null)
    {
        // Collect existing components
        var components = new List<Component>();
        
        if (_entityToArchetype.TryGetValue(entityId, out var currentArchetype))
        {
            // Get all existing components
            foreach (var componentType in targetArchetype.GetComponentTypes())
            {
                if (newComponent != null && componentType == newComponent.GetType())
                    continue; // Skip the new component, we'll add it separately
                    
                var existingComponent = currentArchetype.GetComponent<Component>(entityId);
                if (existingComponent != null)
                {
                    components.Add(existingComponent);
                }
            }
            
            // Remove from current archetype
            currentArchetype.RemoveEntity(entityId);
        }
        
        // Add new component if provided
        if (newComponent != null)
        {
            components.Add(newComponent);
        }
        
        // Add to target archetype
        targetArchetype.AddEntity(entityId, components.ToArray());
        _entityToArchetype[entityId] = targetArchetype;
    }

    private void RemoveEntityCompletely(EntityId entityId)
    {
        if (_entityToArchetype.TryGetValue(entityId, out var archetype))
        {
            archetype.RemoveEntity(entityId);
            _entityToArchetype.Remove(entityId);
        }
        
        _entityComponents.Remove(entityId);
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(ComponentStorage));
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _archetypes.Clear();
            _entityToArchetype.Clear();
            _entityComponents.Clear();
            _disposed = true;
        }
    }
}

