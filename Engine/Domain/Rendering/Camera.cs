using Engine.Domain.Rendering.ValueObjects;

namespace Engine.Domain.Rendering;

/// <summary>
/// Entity representing camera with view/projection
/// </summary>
public class Camera
{
    public string Id { get; set; } = string.Empty;
    public Vector3 Position { get; set; }
    public Vector3 Rotation { get; set; }
    public float FieldOfView { get; set; } = 60.0f;
    public float NearPlane { get; set; } = 0.1f;
    public float FarPlane { get; set; } = 1000.0f;
    public Matrix4x4 ViewMatrix { get; set; } = Matrix4x4.Identity;
    public Matrix4x4 ProjectionMatrix { get; set; } = Matrix4x4.Identity;
}

