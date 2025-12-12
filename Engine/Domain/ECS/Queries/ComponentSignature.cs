using System;
using System.Collections.Generic;
using System.Linq;
using Engine.Domain.ECS;

namespace Engine.Domain.ECS.Queries;

/// <summary>
/// Represents the component signature of an entity archetype
/// Used for efficient grouping of entities with identical component sets
/// </summary>
public readonly struct ComponentSignature : IEquatable<ComponentSignature>
{
    private readonly ulong _signature;
    
    public ComponentSignature(IEnumerable<Type> componentTypes)
    {
        _signature = ComputeSignature(componentTypes);
    }
    
    public ComponentSignature(params Type[] componentTypes) 
        : this((IEnumerable<Type>)componentTypes)
    {
    }
    
    private ComponentSignature(ulong signature)
    {
        _signature = signature;
    }
    
    /// <summary>
    /// Create a signature from a single component type
    /// </summary>
    public static ComponentSignature From<T>() where T : Component
    {
        return new ComponentSignature(typeof(T));
    }
    
    /// <summary>
    /// Create a signature from two component types
    /// </summary>
    public static ComponentSignature From<T1, T2>() 
        where T1 : Component 
        where T2 : Component
    {
        return new ComponentSignature(typeof(T1), typeof(T2));
    }
    
    /// <summary>
    /// Create a signature from three component types
    /// </summary>
    public static ComponentSignature From<T1, T2, T3>() 
        where T1 : Component 
        where T2 : Component 
        where T3 : Component
    {
        return new ComponentSignature(typeof(T1), typeof(T2), typeof(T3));
    }
    
    /// <summary>
    /// Add a component type to this signature
    /// </summary>
    public ComponentSignature Add<T>() where T : Component
    {
        return Add(typeof(T));
    }
    
    /// <summary>
    /// Add a component type to this signature
    /// </summary>
    public ComponentSignature Add(Type componentType)
    {
        if (!typeof(Component).IsAssignableFrom(componentType))
            throw new ArgumentException($"Type {componentType.Name} is not a Component", nameof(componentType));
            
        var typeHash = (ulong)componentType.GetHashCode();
        var newSignature = _signature | (1UL << (int)(typeHash % 64));
        return new ComponentSignature(newSignature);
    }
    
    /// <summary>
    /// Remove a component type from this signature
    /// </summary>
    public ComponentSignature Remove<T>() where T : Component
    {
        return Remove(typeof(T));
    }
    
    /// <summary>
    /// Remove a component type from this signature
    /// </summary>
    public ComponentSignature Remove(Type componentType)
    {
        if (!typeof(Component).IsAssignableFrom(componentType))
            throw new ArgumentException($"Type {componentType.Name} is not a Component", nameof(componentType));
            
        var typeHash = (ulong)componentType.GetHashCode();
        var newSignature = _signature & ~(1UL << (int)(typeHash % 64));
        return new ComponentSignature(newSignature);
    }
    
    /// <summary>
    /// Check if this signature contains a specific component type
    /// </summary>
    public bool Contains<T>() where T : Component
    {
        return Contains(typeof(T));
    }
    
    /// <summary>
    /// Check if this signature contains a specific component type
    /// </summary>
    public bool Contains(Type componentType)
    {
        if (!typeof(Component).IsAssignableFrom(componentType))
            return false;
            
        var typeHash = (ulong)componentType.GetHashCode();
        var bit = 1UL << (int)(typeHash % 64);
        return (_signature & bit) != 0;
    }
    
    /// <summary>
    /// Check if this signature is a superset of another signature (contains all its components)
    /// </summary>
    public bool IsSuperset(ComponentSignature other)
    {
        return (_signature & other._signature) == other._signature;
    }
    
    /// <summary>
    /// Check if this signature is a subset of another signature (all its components are contained)
    /// </summary>
    public bool IsSubset(ComponentSignature other)
    {
        return other.IsSuperset(this);
    }
    
    /// <summary>
    /// Get the intersection of two signatures
    /// </summary>
    public ComponentSignature Intersect(ComponentSignature other)
    {
        return new ComponentSignature(_signature & other._signature);
    }
    
    /// <summary>
    /// Get the union of two signatures
    /// </summary>
    public ComponentSignature Union(ComponentSignature other)
    {
        return new ComponentSignature(_signature | other._signature);
    }
    
    private static ulong ComputeSignature(IEnumerable<Type> componentTypes)
    {
        ulong signature = 0;
        foreach (var type in componentTypes)
        {
            if (!typeof(Component).IsAssignableFrom(type))
                throw new ArgumentException($"Type {type.Name} is not a Component");
                
            var typeHash = (ulong)type.GetHashCode();
            signature |= 1UL << (int)(typeHash % 64);
        }
        return signature;
    }
    
    public bool Equals(ComponentSignature other) => _signature == other._signature;
    
    public override bool Equals(object? obj) => obj is ComponentSignature signature && Equals(signature);
    
    public override int GetHashCode() => _signature.GetHashCode();
    
    public static bool operator ==(ComponentSignature left, ComponentSignature right) => left.Equals(right);
    
    public static bool operator !=(ComponentSignature left, ComponentSignature right) => !left.Equals(right);
    
    public override string ToString() => $"ComponentSignature({_signature:X})";
}