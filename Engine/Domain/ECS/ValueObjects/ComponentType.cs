using System;

namespace Engine.Domain.ECS.ValueObjects;

/// <summary>
/// Value object for component type identification
/// </summary>
public readonly record struct ComponentType
{
    public Type Type { get; init; }
    public int Id { get; init; }
    public string Name => Type.Name;

    public ComponentType(Type type, int id)
    {
        Type = type;
        Id = id;
    }
    
    public ComponentType(Type type)
    {
        Type = type;
        Id = type.GetHashCode();
    }

    public static ComponentType Invalid => new(typeof(object), -1);
}

