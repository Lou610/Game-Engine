using System;
using System.Collections.Generic;
using Engine.Domain.ECS.ValueObjects;

namespace Engine.Domain.ECS.Services;

/// <summary>
/// Manages component archetypes
/// </summary>
public class ComponentArchetypeService
{
    private readonly Dictionary<int, HashSet<Type>> _archetypes = new();

    public int RegisterArchetype(params Type[] componentTypes)
    {
        var archetypeId = _archetypes.Count;
        var typeSet = new HashSet<Type>(componentTypes);
        _archetypes[archetypeId] = typeSet;
        return archetypeId;
    }

    public HashSet<Type>? GetArchetype(int archetypeId)
    {
        return _archetypes.TryGetValue(archetypeId, out var types) ? types : null;
    }
}

