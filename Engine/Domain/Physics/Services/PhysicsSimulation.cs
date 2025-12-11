using Engine.Domain.Physics;

namespace Engine.Domain.Physics.Services;

/// <summary>
/// Domain service for physics calculations
/// </summary>
public class PhysicsSimulation
{
    public void Step(PhysicsWorld world, float deltaTime)
    {
        world.Simulate(deltaTime);
    }
}

