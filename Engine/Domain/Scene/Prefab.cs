using System.Collections.Generic;
using Engine.Domain.ECS;
using Engine.Domain.Scene.ValueObjects;

namespace Engine.Domain.Scene;

/// <summary>
/// Aggregate root representing reusable entity templates
/// </summary>
public class Prefab
{
    public PrefabId Id { get; set; }
    public string Name { get; set; } = string.Empty;
    private readonly List<Component> _components = new();

    public Prefab(PrefabId id, string name)
    {
        Id = id;
        Name = name;
    }

    public void AddComponent(Component component)
    {
        _components.Add(component);
    }

    public Entity Instantiate(World world)
    {
        var entity = world.CreateEntity(Name);
        foreach (var component in _components)
        {
            // Component attachment logic would go here
        }
        return entity;
    }
}

