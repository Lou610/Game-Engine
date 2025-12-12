using Engine.Domain.ECS;
using Engine.Domain.ECS.ValueObjects;

namespace Engine.Infrastructure.ECS;

/// <summary>
/// Repository for component persistence
/// </summary>
public class ComponentRepository
{
    private readonly ComponentStorage _storage;

    public ComponentRepository(ComponentStorage storage)
    {
        _storage = storage;
    }

    public void Save<T>(EntityId entityId, T component) where T : Component
    {
        _storage.AddComponent(entityId, component);
    }

    public T? Load<T>(EntityId entityId) where T : Component
    {
        return _storage.GetComponent<T>(entityId);
    }

    public void Delete<T>(EntityId entityId) where T : Component
    {
        _storage.RemoveComponent<T>(entityId);
    }
}

