using NUnit.Framework;
using Engine.Application.ECS;
using Engine.Domain.ECS;
using Engine.Domain.ECS.Components;
using Engine.Domain.ECS.Services;
using Engine.Infrastructure.ECS;
using Engine.Infrastructure.Logging;

namespace GameEngine.Integration.Tests;

[TestFixture]
public class ECSIntegrationTests
{
    [Test]
    public void CreateEntityWithComponent_Integration_EntityAndComponentStored()
    {
        // Arrange
        var world = new World();
        var storage = new ComponentStorage();
        var repository = new ComponentRepository(storage);
        var lifecycleService = new EntityLifecycleService();
        var archetypeService = new ComponentArchetypeService();
        var logger = new Logger();
        var worldService = new WorldService(world, lifecycleService, archetypeService, logger);

        // Act
        var entity = worldService.CreateEntity("TestEntity");
        var transform = new Transform();
        repository.Save(entity.Id, transform);

        // Assert
        var retrieved = repository.Load<Transform>(entity.Id);
        Assert.That(retrieved, Is.Not.Null);
    }
}

