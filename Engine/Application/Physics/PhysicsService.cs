using Engine.Domain.Physics;
using Engine.Domain.Physics.Services;

namespace Engine.Application.Physics;

/// <summary>
/// Application service orchestrating physics
/// </summary>
public class PhysicsService
{
    private readonly PhysicsSimulation _simulation;
    private readonly CollisionDetection _collisionDetection;

    public PhysicsService(PhysicsSimulation simulation, CollisionDetection collisionDetection)
    {
        _simulation = simulation;
        _collisionDetection = collisionDetection;
    }

    public void Update(PhysicsWorld world, float deltaTime)
    {
        _simulation.Step(world, deltaTime);
    }
}

