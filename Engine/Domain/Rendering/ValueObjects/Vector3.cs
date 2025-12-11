namespace Engine.Domain.Rendering.ValueObjects;

/// <summary>
/// 3D vector value object
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
    public static Vector3 Up => new(0, 1, 0);
    public static Vector3 Down => new(0, -1, 0);
    public static Vector3 Forward => new(0, 0, 1);
    public static Vector3 Backward => new(0, 0, -1);
    public static Vector3 Left => new(-1, 0, 0);
    public static Vector3 Right => new(1, 0, 0);
}

