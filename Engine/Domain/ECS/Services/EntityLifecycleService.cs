using System.Collections.Generic;
using Engine.Domain.ECS;
using Engine.Domain.ECS.Events;
using Engine.Domain.ECS.ValueObjects;

namespace Engine.Domain.ECS.Services;

/// <summary>
/// Handles entity creation/destruction
/// </summary>
public class EntityLifecycleService
{
    private readonly List<EntityCreated> _createdEvents = new();
    private readonly List<EntityDestroyed> _destroyedEvents = new();

    public EntityCreated CreateEntity(EntityId id, string name)
    {
        var evt = new EntityCreated(id, name);
        _createdEvents.Add(evt);
        return evt;
    }

    public EntityDestroyed DestroyEntity(EntityId id)
    {
        var evt = new EntityDestroyed(id);
        _destroyedEvents.Add(evt);
        return evt;
    }

    public IEnumerable<EntityCreated> GetCreatedEvents() => _createdEvents;
    public IEnumerable<EntityDestroyed> GetDestroyedEvents() => _destroyedEvents;

    public void ClearEvents()
    {
        _createdEvents.Clear();
        _destroyedEvents.Clear();
    }
}

