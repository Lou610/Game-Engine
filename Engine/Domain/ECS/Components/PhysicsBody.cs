using Engine.Domain.ECS;

namespace Engine.Domain.ECS.Components;

/// <summary>
/// Entity with physics properties
/// </summary>
public class PhysicsBody : Component
{
    public float Mass { get; set; } = 1.0f;
    public bool IsKinematic { get; set; } = false;
    public bool UseGravity { get; set; } = true;
}

