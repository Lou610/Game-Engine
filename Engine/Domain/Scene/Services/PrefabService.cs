using Engine.Domain.ECS;
using Engine.Domain.Scene.Prefabs;

namespace Engine.Domain.Scene.Services;

/// <summary>
/// Prefab domain operations
/// </summary>
public class PrefabService
{
    public Entity InstantiatePrefab(Prefab prefab, World world)
    {
        // Note: Actual instantiation is handled by PrefabInstantiator in Application layer
        // This is a placeholder for domain-level prefab operations
        throw new NotImplementedException("Use PrefabInstantiator for entity instantiation");
    }
}

