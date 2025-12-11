using Engine.Domain.ECS;
using Engine.Domain.ECS.ValueObjects;

namespace Engine.Domain.ECS.Components;

/// <summary>
/// Transform component - value object (position, rotation, scale)
/// </summary>
public class Transform : Component
{
    public Vector3 Position { get; set; }
    public Vector3 Rotation { get; set; }
    public Vector3 Scale { get; set; }

    public Transform()
    {
        Position = Vector3.Zero;
        Rotation = Vector3.Zero;
        Scale = Vector3.One;
    }

    public Transform(Vector3 position, Vector3 rotation, Vector3 scale)
    {
        Position = position;
        Rotation = rotation;
        Scale = scale;
    }
}

/// <summary>
/// Simple Vector3 value object
/// </summary>
public readonly record struct Vector3
{
    public float X { get; init; }
    public float Y { get; init; }
    public float Z { get; init; }

    public Vector3(float x, float y, float z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public static Vector3 Zero => new(0, 0, 0);
    public static Vector3 One => new(1, 1, 1);
}

