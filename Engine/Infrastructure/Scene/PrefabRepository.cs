using System.Collections.Generic;
using Engine.Domain.Scene.Prefabs;
using Engine.Domain.Scene.ValueObjects;

namespace Engine.Infrastructure.Scene;

/// <summary>
/// Repository for prefab persistence
/// </summary>
public class PrefabRepository
{
    private readonly Dictionary<string, Prefab> _prefabs = new();

    public void Save(Prefab prefab)
    {
        _prefabs[prefab.Id.Value] = prefab;
    }

    public Prefab? Load(PrefabId id)
    {
        return _prefabs.TryGetValue(id.Value, out var prefab) ? prefab : null;
    }

    public void Delete(PrefabId id)
    {
        _prefabs.Remove(id.Value);
    }
}

