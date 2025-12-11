using Engine.Domain.ECS;
using Engine.Domain.Scene;

namespace Engine.Domain.Scene.Services;

/// <summary>
/// Prefab domain operations
/// </summary>
public class PrefabService
{
    public Entity InstantiatePrefab(Prefab prefab, World world)
    {
        return prefab.Instantiate(world);
    }
}

