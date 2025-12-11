using System.Collections.Generic;
using Engine.Domain.Scene;
using Engine.Domain.Scene.ValueObjects;
using Engine.Infrastructure.Logging;
using SceneEntity = Engine.Domain.Scene.Scene;

namespace Engine.Application.Scene;

/// <summary>
/// Application service for scene management
/// </summary>
public class SceneManager
{
    private readonly Dictionary<string, SceneEntity> _scenes = new();
    private SceneEntity? _activeScene;
    private readonly Logger _logger;

    public SceneManager(Logger logger)
    {
        _logger = logger;
    }

    public void LoadScene(SceneEntity scene)
    {
        _scenes[scene.Id.Value] = scene;
        _activeScene = scene;
        _logger.Info($"Scene loaded: {scene.Name}");
    }

    public void UnloadScene(SceneId sceneId)
    {
        if (_scenes.Remove(sceneId.Value))
        {
            if (_activeScene?.Id.Value == sceneId.Value)
            {
                _activeScene = null;
            }
            _logger.Info($"Scene unloaded: {sceneId.Value}");
        }
    }

    public SceneEntity? GetActiveScene() => _activeScene;
}

