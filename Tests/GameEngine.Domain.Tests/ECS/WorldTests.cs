using NUnit.Framework;
using Engine.Domain.ECS;

namespace GameEngine.Domain.Tests.ECS;

[TestFixture]
public class WorldTests
{
    [Test]
    public void CreateEntity_WithName_ReturnsEntity()
    {
        // Arrange
        var world = new World();

        // Act
        var entity = world.CreateEntity("TestEntity");

        // Assert
        Assert.That(entity, Is.Not.Null);
        Assert.That(entity.Name, Is.EqualTo("TestEntity"));
    }

    [Test]
    public void DestroyEntity_ExistingEntity_RemovesFromWorld()
    {
        // Arrange
        var world = new World();
        var entity = world.CreateEntity();

        // Act
        world.DestroyEntity(entity.Id);

        // Assert
        Assert.That(world.GetEntity(entity.Id), Is.Null);
    }
}

