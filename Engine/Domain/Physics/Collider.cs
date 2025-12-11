using Engine.Domain.Physics.ValueObjects;

namespace Engine.Domain.Physics;

/// <summary>
/// Entity representing collision shape
/// </summary>
public class Collider
{
    public string Id { get; set; } = string.Empty;
    public CollisionShape Shape { get; set; }
    public PhysicsMaterial Material { get; set; }
}

