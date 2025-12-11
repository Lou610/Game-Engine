using System;
using System.Collections.Generic;
using Engine.Domain.ECS;

namespace Engine.Infrastructure.ECS;

/// <summary>
/// Efficient component storage implementation using archetypes
/// </summary>
public class ComponentStorage
{
    private readonly Dictionary<Type, Dictionary<ulong, Component>> _components = new();

    public void AddComponent<T>(ulong entityId, T component) where T : Component
    {
        var type = typeof(T);
        if (!_components.TryGetValue(type, out var componentMap))
        {
            componentMap = new Dictionary<ulong, Component>();
            _components[type] = componentMap;
        }

        component.EntityId = new Engine.Domain.ECS.ValueObjects.EntityId(entityId);
        componentMap[entityId] = component;
    }

    public T? GetComponent<T>(ulong entityId) where T : Component
    {
        var type = typeof(T);
        if (_components.TryGetValue(type, out var componentMap))
        {
            if (componentMap.TryGetValue(entityId, out var component))
            {
                return component as T;
            }
        }

        return null;
    }

    public void RemoveComponent<T>(ulong entityId) where T : Component
    {
        var type = typeof(T);
        if (_components.TryGetValue(type, out var componentMap))
        {
            componentMap.Remove(entityId);
        }
    }

    public IEnumerable<T> GetAllComponents<T>() where T : Component
    {
        var type = typeof(T);
        if (_components.TryGetValue(type, out var componentMap))
        {
            foreach (var component in componentMap.Values)
            {
                if (component is T typedComponent)
                {
                    yield return typedComponent;
                }
            }
        }
    }
}

