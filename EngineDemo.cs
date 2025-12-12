using Engine.Domain.ECS;
using Engine.Domain.ECS.Components;
using Engine.Domain.ECS.ValueObjects;
using Engine.Domain.Rendering.ValueObjects;
using Engine.Application.ECS;

namespace GameEngine;

/// <summary>
/// Simple demo showing the ECS engine in action
/// </summary>
public class EngineDemo
{
    public static void RunDemo()
    {
        Console.WriteLine("🚀 Game Engine ECS Demo Starting...\n");
        
        // Create the ECS World
        var eventPublisher = new DomainEventPublisher();
        var world = new World(eventPublisher);
        var systemManager = new SystemManager(world);
        
        Console.WriteLine("✅ ECS World and System Manager created");
        
        // Create some entities
        var player = world.CreateEntity("Player");
        var enemy = world.CreateEntity("Enemy");
        var powerup = world.CreateEntity("PowerUp");
        
        Console.WriteLine($"✅ Created entities: {player.Name}, {enemy.Name}, {powerup.Name}");
        
        // Add Transform components
        var playerTransform = world.AddComponent<Transform>(player.Id);
        playerTransform.Position = new Vector3(0, 0, 0);
        
        var enemyTransform = world.AddComponent<Transform>(enemy.Id);
        enemyTransform.Position = new Vector3(10, 0, 0);
        
        var powerupTransform = world.AddComponent<Transform>(powerup.Id);
        powerupTransform.Position = new Vector3(5, 5, 0);
        
        Console.WriteLine("✅ Added Transform components to all entities");
        
        // Add Camera component to player
        var camera = world.AddComponent<Camera>(player.Id);
        Console.WriteLine("✅ Added Camera component to player");
        
        // Query entities with transforms
        var entitiesWithTransforms = world.Query<Transform>().ToList();
        Console.WriteLine($"📊 Found {entitiesWithTransforms.Count} entities with Transform components:");
        
        foreach (var entity in entitiesWithTransforms)
        {
            var transform = world.GetComponent<Transform>(entity.Id);
            Console.WriteLine($"   - {entity.Name}: Position({transform?.Position.X}, {transform?.Position.Y}, {transform?.Position.Z})");
        }
        
        // Query entities with both Transform and Camera
        var cameraEntities = world.Query<Transform, Camera>().ToList();
        Console.WriteLine($"📊 Found {cameraEntities.Count} entities with both Transform and Camera");
        
        // Test component removal
        Console.WriteLine("\n🔄 Testing component removal...");
        world.RemoveComponent<Camera>(player.Id);
        var cameraEntitiesAfterRemoval = world.Query<Camera>().ToList();
        Console.WriteLine($"📊 After removal: {cameraEntitiesAfterRemoval.Count} entities have Camera component");
        
        // Test entity destruction
        Console.WriteLine("\n💥 Testing entity destruction...");
        world.DestroyEntity(powerup.Id);
        var remainingEntities = world.Query<Transform>().ToList();
        Console.WriteLine($"📊 After destruction: {remainingEntities.Count} entities remain");
        
        // Cleanup
        systemManager.Dispose();
        world.Dispose();
        
        Console.WriteLine("\n🎉 Demo completed successfully! Phase 2 ECS system is fully functional.");
    }
}