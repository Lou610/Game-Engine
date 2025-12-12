using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Engine.Domain.ECS;
using Engine.Domain.ECS.ValueObjects;
using Engine.Domain.ECS.Components;
using Engine.Domain.ECS.Events;
using Engine.Domain.Rendering.ValueObjects;
using Engine.Application.ECS;

namespace GameEngine.Integration.Tests;

[TestFixture]
public class ECSPhase2IntegrationTests
{
    private World _world = null!;
    private SystemManager _systemManager = null!;
    private DomainEventPublisher _eventPublisher = null!;

    [SetUp]
    public void SetUp()
    {
        _eventPublisher = new DomainEventPublisher();
        _world = new World(_eventPublisher);
        _systemManager = new SystemManager(_world);
    }

    [TearDown]
    public void TearDown()
    {
        _systemManager?.Dispose();
        _world?.Dispose();
        
        // Create fresh instances for next test
        _eventPublisher = new DomainEventPublisher();
        _world = new World(_eventPublisher);
        _systemManager = new SystemManager(_world);
    }

    [Test]
    public void CompleteECSWorkflow_CreateEntitiesAddComponentsQueryAndProcess()
    {
        // Arrange - Create and register a movement system (check if already exists)
        var movementSystem = new MovementSystem();
        try
        {
            _systemManager.RegisterSystem(movementSystem);
        }
        catch (InvalidOperationException)
        {
            // System already registered, that's fine for this test
        }
        _systemManager.InitializeSystems();

        // Create entities with transform components
        var player = _world.CreateEntity("Player");
        var enemy1 = _world.CreateEntity("Enemy1");
        var enemy2 = _world.CreateEntity("Enemy2");
        var staticObject = _world.CreateEntity("StaticObject");

        // Add Transform components with different positions
        var playerTransform = _world.AddComponent<Transform>(player.Id);
        playerTransform.Position = new Engine.Domain.Rendering.ValueObjects.Vector3(0, 0, 0);

        var enemy1Transform = _world.AddComponent<Transform>(enemy1.Id);
        enemy1Transform.Position = new Engine.Domain.Rendering.ValueObjects.Vector3(10, 0, 0);

        var enemy2Transform = _world.AddComponent<Transform>(enemy2.Id);
        enemy2Transform.Position = new Engine.Domain.Rendering.ValueObjects.Vector3(-5, 0, 0);

        var staticTransform = _world.AddComponent<Transform>(staticObject.Id);
        staticTransform.Position = new Engine.Domain.Rendering.ValueObjects.Vector3(100, 0, 0);

        // Add Movable component to moving entities (but not static object)
        _world.AddComponent<Movable>(player.Id, new Movable { Speed = 5.0f });
        _world.AddComponent<Movable>(enemy1.Id, new Movable { Speed = 2.0f });
        _world.AddComponent<Movable>(enemy2.Id, new Movable { Speed = 3.0f });

        // Act - Run the movement system
        _systemManager.UpdateSystems(1.0f); // 1 second delta time

        // Assert - Check that only movable entities had their positions updated
        var updatedPlayerTransform = _world.GetComponent<Transform>(player.Id)!;
        var updatedEnemy1Transform = _world.GetComponent<Transform>(enemy1.Id)!;
        var updatedEnemy2Transform = _world.GetComponent<Transform>(enemy2.Id)!;
        var updatedStaticTransform = _world.GetComponent<Transform>(staticObject.Id)!;

        // Movable entities should have moved
        Assert.That(updatedPlayerTransform.Position.X, Is.EqualTo(5.0f)); // 0 + 5*1
        Assert.That(updatedEnemy1Transform.Position.X, Is.EqualTo(12.0f)); // 10 + 2*1
        Assert.That(updatedEnemy2Transform.Position.X, Is.EqualTo(-2.0f)); // -5 + 3*1

        // Static object should not have moved
        Assert.That(updatedStaticTransform.Position.X, Is.EqualTo(100.0f));

        // Verify system processed the correct number of entities
        Assert.That(movementSystem.ProcessedEntityCount, Is.EqualTo(3));
    }

    [Test]
    public void DomainEvents_ArePublishedWhenEntitiesAndComponentsChange()
    {
        // Arrange
        var eventLog = new List<string>();
        
        _eventPublisher.Subscribe<EntityCreated>(e => eventLog.Add($"EntityCreated: {e.EntityName}"));
        _eventPublisher.Subscribe<ComponentAdded>(e => eventLog.Add($"ComponentAdded: {e.ComponentType.Name}"));
        _eventPublisher.Subscribe<ComponentRemoved>(e => eventLog.Add($"ComponentRemoved: {e.ComponentType.Name}"));
        _eventPublisher.Subscribe<EntityDestroyed>(e => eventLog.Add($"EntityDestroyed: {e.EntityName}"));

        // Act - Perform ECS operations
        var entity = _world.CreateEntity("TestEntity");
        _world.AddComponent<Transform>(entity.Id);
        _world.AddComponent<Camera>(entity.Id);
        _world.RemoveComponent<Camera>(entity.Id);
        _world.DestroyEntity(entity.Id);

        // Assert - Check events were fired (5 events: Create + 2 AddComponent + RemoveComponent + DestroyEntity)
        Assert.That(eventLog.Count, Is.EqualTo(5));
        Assert.That(eventLog[0], Does.Contain("EntityCreated: TestEntity"));
        Assert.That(eventLog[1], Does.Contain("ComponentAdded: Transform"));
        Assert.That(eventLog[2], Does.Contain("ComponentAdded: Camera"));
        Assert.That(eventLog[3], Does.Contain("ComponentRemoved: Camera"));
        Assert.That(eventLog[4], Does.Contain("EntityDestroyed: TestEntity"));
    }

    [Test]
    public void ComplexEntityQueries_WorkCorrectlyWithArchetypes()
    {
        // Arrange - Create entities with different component combinations
        var renderableEntity = _world.CreateEntity("RenderableEntity");
        _world.AddComponent<Transform>(renderableEntity.Id);
        _world.AddComponent<Renderable>(renderableEntity.Id);

        var movableEntity = _world.CreateEntity("MovableEntity");
        _world.AddComponent<Transform>(movableEntity.Id);
        _world.AddComponent<Movable>(movableEntity.Id);

        var renderableMovableEntity = _world.CreateEntity("RenderableMovableEntity");
        _world.AddComponent<Transform>(renderableMovableEntity.Id);
        _world.AddComponent<Renderable>(renderableMovableEntity.Id);
        _world.AddComponent<Movable>(renderableMovableEntity.Id);

        var cameraEntity = _world.CreateEntity("CameraEntity");
        _world.AddComponent<Transform>(cameraEntity.Id);
        _world.AddComponent<Camera>(cameraEntity.Id);

        // Act & Assert - Test various queries
        
        // All entities with Transform
        var transformEntities = _world.Query<Transform>().ToList();
        Assert.That(transformEntities.Count, Is.EqualTo(4));

        // Only renderable entities
        var renderableEntities = _world.Query<Renderable>().ToList();
        Assert.That(renderableEntities.Count, Is.EqualTo(2));
        Assert.That(renderableEntities.Select(e => e.Name), 
            Does.Contain("RenderableEntity").And.Contain("RenderableMovableEntity"));

        // Entities with Transform and Renderable
        var renderableTransformEntities = _world.Query<Transform, Renderable>().ToList();
        Assert.That(renderableTransformEntities.Count, Is.EqualTo(2));

        // Entities with just Transform (check if all have it)
        var basicEntities = _world.Query<Transform>().ToList();
        Assert.That(basicEntities.Count, Is.EqualTo(4));
    }

    // Test components and systems for integration testing

    public class Movable : Component
    {
        public float Speed { get; set; } = 1.0f;
    }

    public class MovementSystem : Engine.Application.ECS.System
    {
        public int ProcessedEntityCount { get; private set; }

        public override void Update(float deltaTime)
        {
            ProcessedEntityCount = 0;

            // Query entities with both Transform and Movable components
            foreach (var (entity, transform, movable) in GetEntitiesWithComponents<Transform, Movable>())
            {
                // Move the entity forward based on its speed
                transform.Position = new Engine.Domain.Rendering.ValueObjects.Vector3(
                    transform.Position.X + movable.Speed * deltaTime,
                    transform.Position.Y,
                    transform.Position.Z
                );

                ProcessedEntityCount++;
            }
        }

        // Helper method to get entities with multiple components
        private IEnumerable<(Entity Entity, Transform Transform, Movable Movable)> GetEntitiesWithComponents<T1, T2>()
            where T1 : Component
            where T2 : Component
        {
            foreach (var entity in Query<T1, T2>())
            {
                var transform = World?.GetComponent<Transform>(entity.Id);
                var movable = World?.GetComponent<Movable>(entity.Id);
                
                if (transform != null && movable != null)
                {
                    yield return (entity, transform, movable);
                }
            }
        }
    }
}