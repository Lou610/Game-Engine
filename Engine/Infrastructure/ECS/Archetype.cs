using System;
using System.Collections.Generic;
using System.Linq;
using Engine.Domain.ECS;
using Engine.Domain.ECS.ValueObjects;
using Engine.Domain.ECS.Queries;

namespace Engine.Infrastructure.ECS;

/// <summary>
/// Archetype represents a group of entities with identical component signatures
/// Provides cache-friendly storage and efficient iteration over components
/// </summary>
public class Archetype
{
    public ComponentSignature Signature { get; }
    
    private readonly Dictionary<Type, Array> _componentArrays;
    private readonly List<EntityId> _entities;
    private readonly Dictionary<EntityId, int> _entityToIndex;
    private readonly Dictionary<Type, int> _componentTypeToIndex;
    private int _entityCount;
    
    public int EntityCount => _entityCount;
    public IReadOnlyList<EntityId> Entities => _entities.AsReadOnly();
    
    public Archetype(ComponentSignature signature, IEnumerable<Type> componentTypes)
    {
        Signature = signature;
        _componentArrays = new Dictionary<Type, Array>();
        _entities = new List<EntityId>();
        _entityToIndex = new Dictionary<EntityId, int>();
        _componentTypeToIndex = new Dictionary<Type, int>();
        
        // Initialize component arrays
        var types = componentTypes.ToArray();
        for (int i = 0; i < types.Length; i++)
        {
            var componentType = types[i];
            _componentTypeToIndex[componentType] = i;
            
            // Create array for this component type with initial capacity
            var array = Array.CreateInstance(componentType, 64); // Start with 64 entities capacity
            _componentArrays[componentType] = array;
        }
        
        _entityCount = 0;
    }
    
    /// <summary>
    /// Add an entity to this archetype with its components
    /// </summary>
    public void AddEntity(EntityId entityId, params Component[] components)
    {
        if (_entityToIndex.ContainsKey(entityId))
            throw new InvalidOperationException($"Entity {entityId} already exists in this archetype");
            
        // Ensure capacity
        EnsureCapacity(_entityCount + 1);
        
        // Add entity
        _entities.Add(entityId);
        _entityToIndex[entityId] = _entityCount;
        
        // Add components
        foreach (var component in components)
        {
            var componentType = component.GetType();
            
            if (!_componentArrays.ContainsKey(componentType))
                throw new InvalidOperationException($"Component type {componentType.Name} is not part of this archetype signature");
                
            var array = _componentArrays[componentType];
            array.SetValue(component, _entityCount);
        }
        
        _entityCount++;
    }
    
    /// <summary>
    /// Remove an entity from this archetype
    /// </summary>
    public void RemoveEntity(EntityId entityId)
    {
        if (!_entityToIndex.TryGetValue(entityId, out int index))
            throw new InvalidOperationException($"Entity {entityId} does not exist in this archetype");
            
        int lastIndex = _entityCount - 1;
        
        if (index != lastIndex)
        {
            // Swap with last entity to maintain contiguous storage
            var lastEntityId = _entities[lastIndex];
            
            // Move entity
            _entities[index] = lastEntityId;
            _entityToIndex[lastEntityId] = index;
            
            // Move all component data
            foreach (var (componentType, array) in _componentArrays)
            {
                var lastComponent = array.GetValue(lastIndex);
                array.SetValue(lastComponent, index);
            }
        }
        
        // Remove last element
        _entities.RemoveAt(lastIndex);
        _entityToIndex.Remove(entityId);
        _entityCount--;
    }
    
    /// <summary>
    /// Get component array for a specific component type
    /// </summary>
    public T[] GetComponentArray<T>() where T : Component
    {
        var componentType = typeof(T);
        if (!_componentArrays.TryGetValue(componentType, out var array))
            throw new InvalidOperationException($"Component type {componentType.Name} is not part of this archetype");
            
        return (T[])array;
    }
    
    /// <summary>
    /// Get a specific component for an entity
    /// </summary>
    public T? GetComponent<T>(EntityId entityId) where T : Component
    {
        if (!_entityToIndex.TryGetValue(entityId, out int index))
            return null;
            
        var componentType = typeof(T);
        if (!_componentArrays.TryGetValue(componentType, out var array))
            return null;
            
        return (T?)array.GetValue(index);
    }
    
    /// <summary>
    /// Set a component for an entity
    /// </summary>
    public void SetComponent<T>(EntityId entityId, T component) where T : Component
    {
        if (!_entityToIndex.TryGetValue(entityId, out int index))
            throw new InvalidOperationException($"Entity {entityId} does not exist in this archetype");
            
        var componentType = typeof(T);
        if (!_componentArrays.TryGetValue(componentType, out var array))
            throw new InvalidOperationException($"Component type {componentType.Name} is not part of this archetype");
            
        array.SetValue(component, index);
    }
    
    /// <summary>
    /// Check if this archetype contains an entity
    /// </summary>
    public bool ContainsEntity(EntityId entityId)
    {
        return _entityToIndex.ContainsKey(entityId);
    }
    
    /// <summary>
    /// Get all component types in this archetype
    /// </summary>
    public IEnumerable<Type> GetComponentTypes()
    {
        return _componentArrays.Keys;
    }
    
    /// <summary>
    /// Iterate over all entities and their components of a specific type
    /// </summary>
    public IEnumerable<(EntityId EntityId, T Component)> IterateComponents<T>() where T : Component
    {
        var components = GetComponentArray<T>();
        for (int i = 0; i < _entityCount; i++)
        {
            yield return (_entities[i], components[i]);
        }
    }
    
    /// <summary>
    /// Iterate over all entities and their two component types
    /// </summary>
    public IEnumerable<(EntityId EntityId, T1 Component1, T2 Component2)> IterateComponents<T1, T2>() 
        where T1 : Component 
        where T2 : Component
    {
        var components1 = GetComponentArray<T1>();
        var components2 = GetComponentArray<T2>();
        
        for (int i = 0; i < _entityCount; i++)
        {
            yield return (_entities[i], components1[i], components2[i]);
        }
    }
    
    /// <summary>
    /// Iterate over all entities and their three component types
    /// </summary>
    public IEnumerable<(EntityId EntityId, T1 Component1, T2 Component2, T3 Component3)> IterateComponents<T1, T2, T3>() 
        where T1 : Component 
        where T2 : Component 
        where T3 : Component
    {
        var components1 = GetComponentArray<T1>();
        var components2 = GetComponentArray<T2>();
        var components3 = GetComponentArray<T3>();
        
        for (int i = 0; i < _entityCount; i++)
        {
            yield return (_entities[i], components1[i], components2[i], components3[i]);
        }
    }
    
    private void EnsureCapacity(int requiredCapacity)
    {
        if (_componentArrays.Count == 0) return;
        
        var currentCapacity = _componentArrays.Values.First().Length;
        if (currentCapacity >= requiredCapacity) return;
        
        // Double capacity
        int newCapacity = Math.Max(currentCapacity * 2, requiredCapacity);
        
        // Resize all component arrays
        foreach (var (componentType, oldArray) in _componentArrays.ToList())
        {
            var newArray = Array.CreateInstance(componentType, newCapacity);
            Array.Copy(oldArray, newArray, _entityCount);
            _componentArrays[componentType] = newArray;
        }
    }
    
    public override string ToString()
    {
        var componentNames = _componentArrays.Keys.Select(t => t.Name);
        return $"Archetype({string.Join(", ", componentNames)}) - {_entityCount} entities";
    }
}