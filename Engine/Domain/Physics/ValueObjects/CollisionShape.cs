namespace Engine.Domain.Physics.ValueObjects;

/// <summary>
/// Collision shape definition
/// </summary>
public readonly record struct CollisionShape
{
    public ShapeType Type { get; init; }
    public float Width { get; init; }
    public float Height { get; init; }
    public float Depth { get; init; }
    public float Radius { get; init; }

    public CollisionShape(ShapeType type, float width = 0, float height = 0, float depth = 0, float radius = 0)
    {
        Type = type;
        Width = width;
        Height = height;
        Depth = depth;
        Radius = radius;
    }
}

public enum ShapeType
{
    Box,
    Sphere,
    Capsule,
    Plane
}

