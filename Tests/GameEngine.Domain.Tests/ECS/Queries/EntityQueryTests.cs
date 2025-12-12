using System;
using System.Linq;
using NUnit.Framework;
using Engine.Domain.ECS;
using Engine.Domain.ECS.ValueObjects;
using Engine.Domain.ECS.Queries;
using Engine.Domain.ECS.Components;
using Engine.Application.ECS;

namespace GameEngine.Domain.Tests.ECS.Queries;

[TestFixture]
public class EntityQueryTests
{
    private World _world = null!;
    private DomainEventPublisher _eventPublisher = null!;

    [SetUp]
    public void SetUp()
    {
        _eventPublisher = new DomainEventPublisher();
        _world = new World(_eventPublisher);
    }

    [TearDown]
    public void TearDown()
    {
        _world?.Dispose();
    }

    [Test]
    public void Query_WithSingleComponent_ReturnsEntitiesWithThatComponent()
    {
        // Arrange
        var entity1 = _world.CreateEntity("Entity1");
        var entity2 = _world.CreateEntity("Entity2");
        var entity3 = _world.CreateEntity("Entity3");

        _world.AddComponent<Transform>(entity1.Id);
        _world.AddComponent<Transform>(entity2.Id);
        // entity3 has no Transform

        // Act
        var results = _world.Query().With<Transform>().Execute().ToList();

        // Assert
        Assert.That(results.Count, Is.EqualTo(2));
        Assert.That(results, Contains.Item(entity1));
        Assert.That(results, Contains.Item(entity2));
        Assert.That(results, Does.Not.Contain(entity3));
    }

    [Test]
    public void Query_WithMultipleComponents_ReturnsEntitiesWithAllComponents()
    {
        // Arrange
        var entity1 = _world.CreateEntity("Entity1");
        var entity2 = _world.CreateEntity("Entity2");
        var entity3 = _world.CreateEntity("Entity3");

        _world.AddComponent<Transform>(entity1.Id);
        _world.AddComponent<Camera>(entity1.Id);

        _world.AddComponent<Transform>(entity2.Id);
        // entity2 missing Camera

        _world.AddComponent<Camera>(entity3.Id);
        // entity3 missing Transform

        // Act
        var results = _world.Query().With<Transform, Camera>().Execute().ToList();

        // Assert
        Assert.That(results.Count, Is.EqualTo(1));
        Assert.That(results, Contains.Item(entity1));
    }

    [Test]
    public void Query_WithoutComponent_ExcludesEntitiesWithThatComponent()
    {
        // Arrange
        var entity1 = _world.CreateEntity("Entity1");
        var entity2 = _world.CreateEntity("Entity2");

        _world.AddComponent<Transform>(entity1.Id);
        _world.AddComponent<Camera>(entity1.Id);

        _world.AddComponent<Transform>(entity2.Id);
        // entity2 has no Camera

        // Act
        var results = _world.Query().With<Transform>().Without<Camera>().Execute().ToList();

        // Assert
        Assert.That(results.Count, Is.EqualTo(1));
        Assert.That(results, Contains.Item(entity2));
    }

    [Test]
    public void Query_ExecuteWithComponent_ReturnsEntitiesAndComponents()
    {
        // Arrange
        var entity1 = _world.CreateEntity("Entity1");
        var entity2 = _world.CreateEntity("Entity2");

        var transform1 = _world.AddComponent<Transform>(entity1.Id);
        var transform2 = _world.AddComponent<Transform>(entity2.Id);

        // Act
        var results = _world.Query().With<Transform>().ExecuteWithComponent<Transform>().ToList();

        // Assert
        Assert.That(results.Count, Is.EqualTo(2));
        
        var result1 = results.First(r => r.Entity.Id == entity1.Id);
        var result2 = results.First(r => r.Entity.Id == entity2.Id);
        
        Assert.That(result1.Component, Is.EqualTo(transform1));
        Assert.That(result2.Component, Is.EqualTo(transform2));
    }

    [Test]
    public void Query_Count_ReturnsCorrectCount()
    {
        // Arrange
        _world.CreateEntity("Entity1");
        var entity2 = _world.CreateEntity("Entity2");
        var entity3 = _world.CreateEntity("Entity3");

        _world.AddComponent<Transform>(entity2.Id);
        _world.AddComponent<Transform>(entity3.Id);

        // Act
        var count = _world.Query().With<Transform>().Count();

        // Assert
        Assert.That(count, Is.EqualTo(2));
    }

    [Test]
    public void Query_Any_ReturnsTrueWhenEntitiesExist()
    {
        // Arrange
        var entity = _world.CreateEntity("Entity1");
        _world.AddComponent<Transform>(entity.Id);

        // Act & Assert
        Assert.That(_world.Query().With<Transform>().Any(), Is.True);
        Assert.That(_world.Query().With<Camera>().Any(), Is.False);
    }

    [Test]
    public void Query_First_ReturnsFirstMatchingEntity()
    {
        // Arrange
        var entity1 = _world.CreateEntity("Entity1");
        var entity2 = _world.CreateEntity("Entity2");

        _world.AddComponent<Transform>(entity1.Id);
        _world.AddComponent<Transform>(entity2.Id);

        // Act
        var result = _world.Query().With<Transform>().First();

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Id, Is.EqualTo(entity1.Id).Or.EqualTo(entity2.Id));
    }

    [Test]
    public void Query_Single_ThrowsWhenMultipleEntitiesMatch()
    {
        // Arrange
        var entity1 = _world.CreateEntity("Entity1");
        var entity2 = _world.CreateEntity("Entity2");

        _world.AddComponent<Transform>(entity1.Id);
        _world.AddComponent<Transform>(entity2.Id);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => _world.Query().With<Transform>().Single());
    }

    [Test]
    public void Query_Single_ThrowsWhenNoEntitiesMatch()
    {
        // Arrange
        _world.CreateEntity("Entity1");

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => _world.Query().With<Transform>().Single());
    }

    [Test]
    public void Query_Single_ReturnsEntityWhenExactlyOneMatches()
    {
        // Arrange
        var entity = _world.CreateEntity("Entity1");
        _world.AddComponent<Transform>(entity.Id);

        // Act
        var result = _world.Query().With<Transform>().Single();

        // Assert
        Assert.That(result.Id, Is.EqualTo(entity.Id));
    }
}