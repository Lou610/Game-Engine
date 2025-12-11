using System.Collections.Generic;
using Engine.Domain.Scene;
using Engine.Domain.Scene.ValueObjects;

namespace Engine.Infrastructure.Scene;

/// <summary>
/// Repository for scene persistence
/// </summary>
public class SceneRepository
{
    private readonly Dictionary<string, Scene> _scenes = new();

    public void Save(Scene scene)
    {
        _scenes[scene.Id.Value] = scene;
    }

    public Scene? Load(SceneId id)
    {
        return _scenes.TryGetValue(id.Value, out var scene) ? scene : null;
    }

    public void Delete(SceneId id)
    {
        _scenes.Remove(id.Value);
    }

    public IEnumerable<Scene> GetAll()
    {
        return _scenes.Values;
    }
}

