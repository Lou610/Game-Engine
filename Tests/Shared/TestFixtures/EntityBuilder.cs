using Engine.Domain.ECS;
using Engine.Domain.ECS.ValueObjects;

namespace Tests.Shared.TestFixtures;

/// <summary>
/// Fluent builder for creating test entities
/// </summary>
public class EntityBuilder
{
    private EntityId _id = new(1);
    private string _name = "TestEntity";

    public EntityBuilder WithId(EntityId id)
    {
        _id = id;
        return this;
    }

    public EntityBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public Entity Build()
    {
        return new Entity(_id, _name);
    }
}

