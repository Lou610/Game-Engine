namespace Engine.Domain.Physics.ValueObjects;

/// <summary>
/// Material properties (friction, restitution)
/// </summary>
public readonly record struct PhysicsMaterial
{
    public float Friction { get; init; }
    public float Restitution { get; init; }
    public float Density { get; init; }

    public PhysicsMaterial(float friction = 0.5f, float restitution = 0.0f, float density = 1.0f)
    {
        Friction = friction;
        Restitution = restitution;
        Density = density;
    }
}

