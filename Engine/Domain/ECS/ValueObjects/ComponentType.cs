using System;

namespace Engine.Domain.ECS.ValueObjects;

/// <summary>
/// Value object for component type identification
/// </summary>
public readonly record struct ComponentType
{
    public Type Type { get; init; }
    public int Id { get; init; }

    public ComponentType(Type type, int id)
    {
        Type = type;
        Id = id;
    }

    public static ComponentType Invalid => new(typeof(object), -1);
}

