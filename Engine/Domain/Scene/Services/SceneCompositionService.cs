using Engine.Domain.ECS;
using Engine.Domain.Scene;

namespace Engine.Domain.Scene.Services;

/// <summary>
/// Manages scene composition
/// </summary>
public class SceneCompositionService
{
    public void ComposeScene(Scene scene, Entity entity)
    {
        scene.AddEntity(entity);
    }

    public void DecomposeScene(Scene scene, Entity entity)
    {
        scene.RemoveEntity(entity);
    }
}

