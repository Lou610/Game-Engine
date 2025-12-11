using Engine.Domain.Physics.ValueObjects;

namespace Engine.Domain.Physics;

/// <summary>
/// Entity with physics body properties
/// </summary>
public class Rigidbody
{
    public string Id { get; set; } = string.Empty;
    public float Mass { get; set; } = 1.0f;
    public bool IsKinematic { get; set; } = false;
    public Force Velocity { get; set; } = Force.Zero;
}

