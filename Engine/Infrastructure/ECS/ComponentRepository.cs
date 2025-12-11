using Engine.Domain.ECS;

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

    public void Save<T>(ulong entityId, T component) where T : Component
    {
        _storage.AddComponent(entityId, component);
    }

    public T? Load<T>(ulong entityId) where T : Component
    {
        return _storage.GetComponent<T>(entityId);
    }

    public void Delete<T>(ulong entityId) where T : Component
    {
        _storage.RemoveComponent<T>(entityId);
    }
}

