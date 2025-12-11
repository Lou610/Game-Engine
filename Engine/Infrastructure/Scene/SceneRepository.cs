using System.Collections.Generic;
using Engine.Domain.Scene;
using Engine.Domain.Scene.ValueObjects;
using SceneEntity = Engine.Domain.Scene.Scene;

namespace Engine.Infrastructure.Scene;

/// <summary>
/// Repository for scene persistence
/// </summary>
public class SceneRepository
{
    private readonly Dictionary<string, SceneEntity> _scenes = new();

    public void Save(SceneEntity scene)
    {
        _scenes[scene.Id.Value] = scene;
    }

    public SceneEntity? Load(SceneId id)
    {
        return _scenes.TryGetValue(id.Value, out var scene) ? scene : null;
    }

    public void Delete(SceneId id)
    {
        _scenes.Remove(id.Value);
    }

    public IEnumerable<SceneEntity> GetAll()
    {
        return _scenes.Values;
    }
}

