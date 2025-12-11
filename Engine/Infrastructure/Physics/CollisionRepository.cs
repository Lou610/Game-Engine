using System.Collections.Generic;
using Engine.Domain.Physics;
using Engine.Domain.Physics.Services;

namespace Engine.Infrastructure.Physics;

/// <summary>
/// Collision data persistence
/// </summary>
public class CollisionRepository
{
    private readonly List<CollisionPair> _collisions = new();

    public void Save(CollisionPair collision)
    {
        _collisions.Add(collision);
    }

    public IEnumerable<CollisionPair> GetAll()
    {
        return _collisions;
    }
}

