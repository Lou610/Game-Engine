using System.Collections.Generic;
using Engine.Domain.ECS.ValueObjects;

namespace Engine.Domain.ECS;

/// <summary>
/// Aggregate root managing entities and components
/// </summary>
public class World
{
    private readonly Dictionary<EntityId, Entity> _entities = new();
    private ulong _nextEntityId = 1;

    public Entity CreateEntity(string name = "")
    {
        var id = new EntityId(_nextEntityId++);
        var entity = new Entity(id, name);
        _entities[id] = entity;
        return entity;
    }

    public void DestroyEntity(EntityId id)
    {
        _entities.Remove(id);
    }

    public Entity? GetEntity(EntityId id)
    {
        return _entities.TryGetValue(id, out var entity) ? entity : null;
    }

    public IEnumerable<Entity> GetAllEntities() => _entities.Values;
}

