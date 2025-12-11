using Engine.Domain.ECS;

namespace Engine.Domain.ECS.Components;

/// <summary>
/// Entity representing light source
/// </summary>
public class Light : Component
{
    public LightType Type { get; set; } = LightType.Directional;
    public Color Color { get; set; } = Color.White;
    public float Intensity { get; set; } = 1.0f;
}

public enum LightType
{
    Directional,
    Point,
    Spot
}

public readonly record struct Color
{
    public float R { get; init; }
    public float G { get; init; }
    public float B { get; init; }
    public float A { get; init; }

    public Color(float r, float g, float b, float a = 1.0f)
    {
        R = r;
        G = g;
        B = b;
        A = a;
    }

    public static Color White => new(1, 1, 1, 1);
    public static Color Black => new(0, 0, 0, 1);
}

