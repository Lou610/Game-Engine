using System.Collections.Generic;
using Engine.Domain.Physics;

namespace Engine.Domain.Physics.Services;

/// <summary>
/// Domain service for collision detection
/// </summary>
public class CollisionDetection
{
    public IEnumerable<CollisionPair> DetectCollisions(IEnumerable<Collider> colliders)
    {
        // Collision detection logic
        yield break;
    }
}

public record CollisionPair(Collider A, Collider B);

