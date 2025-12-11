using NUnit.Framework;
using Engine.Domain.ECS;
using Engine.Domain.ECS.Components;
using Engine.Domain.ECS.ValueObjects;

namespace GameEngine.Domain.Tests.ECS;

[TestFixture]
public class ComponentTests
{
    [Test]
    public void Transform_DefaultConstructor_SetsDefaultValues()
    {
        // Act
        var transform = new Transform();

        // Assert
        Assert.That(transform.Position, Is.EqualTo(Vector3.Zero));
        Assert.That(transform.Rotation, Is.EqualTo(Vector3.Zero));
        Assert.That(transform.Scale, Is.EqualTo(Vector3.One));
    }
}

