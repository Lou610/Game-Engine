namespace Engine.Domain.Rendering.ValueObjects;

/// <summary>
/// 4x4 matrix value object
/// </summary>
public readonly record struct Matrix4x4
{
    public float M11 { get; init; }
    public float M12 { get; init; }
    public float M13 { get; init; }
    public float M14 { get; init; }
    public float M21 { get; init; }
    public float M22 { get; init; }
    public float M23 { get; init; }
    public float M24 { get; init; }
    public float M31 { get; init; }
    public float M32 { get; init; }
    public float M33 { get; init; }
    public float M34 { get; init; }
    public float M41 { get; init; }
    public float M42 { get; init; }
    public float M43 { get; init; }
    public float M44 { get; init; }

    public Matrix4x4(
        float m11, float m12, float m13, float m14,
        float m21, float m22, float m23, float m24,
        float m31, float m32, float m33, float m34,
        float m41, float m42, float m43, float m44)
    {
        M11 = m11; M12 = m12; M13 = m13; M14 = m14;
        M21 = m21; M22 = m22; M23 = m23; M24 = m24;
        M31 = m31; M32 = m32; M33 = m33; M34 = m34;
        M41 = m41; M42 = m42; M43 = m43; M44 = m44;
    }

    public static Matrix4x4 Identity => new(
        1, 0, 0, 0,
        0, 1, 0, 0,
        0, 0, 1, 0,
        0, 0, 0, 1
    );
}

