using Engine.Domain.ECS;

namespace Engine.Domain.ECS.Components;

/// <summary>
/// Entity representing camera
/// </summary>
public class Camera : Component
{
    public float FieldOfView { get; set; } = 60.0f;
    public float NearPlane { get; set; } = 0.1f;
    public float FarPlane { get; set; } = 1000.0f;
    public bool IsMainCamera { get; set; } = false;
}

