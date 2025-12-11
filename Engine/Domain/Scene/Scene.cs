using System.Collections.Generic;
using Engine.Domain.ECS;
using Engine.Domain.Scene.ValueObjects;

namespace Engine.Domain.Scene;

/// <summary>
/// Aggregate root containing entities and systems
/// </summary>
public class Scene
{
    public SceneId Id { get; set; }
    public string Name { get; set; } = string.Empty;
    private readonly List<Entity> _entities = new();
    private readonly World _world;

    public Scene(SceneId id, string name, World world)
    {
        Id = id;
        Name = name;
        _world = world;
    }

    public void AddEntity(Entity entity)
    {
        _entities.Add(entity);
    }

    public void RemoveEntity(Entity entity)
    {
        _entities.Remove(entity);
    }

    public IEnumerable<Entity> GetEntities() => _entities;
}

