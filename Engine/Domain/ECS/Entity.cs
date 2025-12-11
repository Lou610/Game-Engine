using Engine.Domain.ECS.ValueObjects;

namespace Engine.Domain.ECS;

/// <summary>
/// Aggregate root with unique identity
/// </summary>
public class Entity
{
    public EntityId Id { get; }
    public string Name { get; set; }

    public Entity(EntityId id, string name = "")
    {
        Id = id;
        Name = string.IsNullOrEmpty(name) ? $"Entity_{id.Value}" : name;
    }

    public override bool Equals(object? obj)
    {
        return obj is Entity entity && Id == entity.Id;
    }

    public override int GetHashCode() => Id.GetHashCode();
}

