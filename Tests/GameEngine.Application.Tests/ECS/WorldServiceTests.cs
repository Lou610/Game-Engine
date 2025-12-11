using NUnit.Framework;
using Engine.Application.ECS;
using Engine.Domain.ECS;
using Engine.Domain.ECS.Services;
using Engine.Infrastructure.Logging;

namespace GameEngine.Application.Tests.ECS;

[TestFixture]
public class WorldServiceTests
{
    [Test]
    public void CreateEntity_ValidName_CreatesEntity()
    {
        // Arrange
        var world = new World();
        var lifecycleService = new EntityLifecycleService();
        var archetypeService = new ComponentArchetypeService();
        var logger = new Logger();
        var worldService = new WorldService(world, lifecycleService, archetypeService, logger);

        // Act
        var entity = worldService.CreateEntity("TestEntity");

        // Assert
        Assert.That(entity, Is.Not.Null);
        Assert.That(entity.Name, Is.EqualTo("TestEntity"));
    }
}

