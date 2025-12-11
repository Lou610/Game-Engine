using NUnit.Framework;
using Engine.Domain.ECS;
using Engine.Domain.ECS.ValueObjects;

namespace GameEngine.Domain.Tests.ECS;

[TestFixture]
public class EntityTests
{
    [Test]
    public void CreateEntity_WithValidData_ReturnsEntityWithId()
    {
        // Arrange
        var id = new EntityId(1);
        var name = "TestEntity";

        // Act
        var entity = new Entity(id, name);

        // Assert
        Assert.That(entity.Id, Is.EqualTo(id));
        Assert.That(entity.Name, Is.EqualTo(name));
    }

    [Test]
    public void Entity_Equality_ComparesById()
    {
        // Arrange
        var id = new EntityId(1);
        var entity1 = new Entity(id, "Entity1");
        var entity2 = new Entity(id, "Entity2");

        // Assert
        Assert.That(entity1, Is.EqualTo(entity2));
    }
}

