namespace Engine.Domain.Physics.ValueObjects;

/// <summary>
/// Force vector value object
/// </summary>
public readonly record struct Force
{
    public float X { get; init; }
    public float Y { get; init; }
    public float Z { get; init; }

    public Force(float x, float y, float z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public static Force Zero => new(0, 0, 0);
}

