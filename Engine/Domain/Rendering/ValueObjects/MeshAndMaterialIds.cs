using System;

namespace Engine.Domain.Rendering.ValueObjects;

/// <summary>
/// Unique identifier for a mesh
/// </summary>
public readonly record struct MeshId(Guid Value)
{
    public static MeshId NewId() => new(Guid.NewGuid());
    public static MeshId Empty => new(Guid.Empty);
    
    public override string ToString() => Value.ToString();
}

/// <summary>
/// Unique identifier for a material
/// </summary>
public readonly record struct MaterialId(Guid Value)
{
    public static MaterialId NewId() => new(Guid.NewGuid());
    public static MaterialId Empty => new(Guid.Empty);
    
    public override string ToString() => Value.ToString();
}