using System.Collections.Generic;
using Engine.Domain.Physics.ValueObjects;

namespace Engine.Domain.Physics;

/// <summary>
/// Aggregate root managing physics simulation
/// </summary>
public class PhysicsWorld
{
    public string Id { get; set; } = string.Empty;
    private readonly List<Collider> _colliders = new();
    private readonly List<Rigidbody> _rigidbodies = new();

    public void AddCollider(Collider collider)
    {
        _colliders.Add(collider);
    }

    public void AddRigidbody(Rigidbody rigidbody)
    {
        _rigidbodies.Add(rigidbody);
    }

    public void Simulate(float deltaTime)
    {
        // Physics simulation logic
    }
}

