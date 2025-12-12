using System;
using System.Collections.Generic;
using System.Linq;
using Engine.Domain.ECS;
using Engine.Domain.ECS.Queries;
using Engine.Infrastructure.ECS;

namespace Engine.Domain.ECS.Queries;

/// <summary>
/// Fluent query builder for complex entity filtering
/// Provides efficient component-based entity selection
/// </summary>
public class EntityQuery
{
    private readonly World _world;
    private readonly HashSet<Type> _withComponents;
    private readonly HashSet<Type> _withoutComponents;
    private readonly Dictionary<Type, object> _componentFilters;
    private bool _cached;
    private IEnumerable<Entity>? _cachedResults;

    internal EntityQuery(World world)
    {
        _world = world ?? throw new ArgumentNullException(nameof(world));
        _withComponents = new HashSet<Type>();
        _withoutComponents = new HashSet<Type>();
        _componentFilters = new Dictionary<Type, object>();
    }

    /// <summary>
    /// Include entities that have the specified component type
    /// </summary>
    public EntityQuery With<T>() where T : Component
    {
        _withComponents.Add(typeof(T));
        InvalidateCache();
        return this;
    }

    /// <summary>
    /// Include entities that have the specified component types
    /// </summary>
    public EntityQuery With<T1, T2>() 
        where T1 : Component 
        where T2 : Component
    {
        _withComponents.Add(typeof(T1));
        _withComponents.Add(typeof(T2));
        InvalidateCache();
        return this;
    }

    /// <summary>
    /// Include entities that have the specified component types
    /// </summary>
    public EntityQuery With<T1, T2, T3>() 
        where T1 : Component 
        where T2 : Component 
        where T3 : Component
    {
        _withComponents.Add(typeof(T1));
        _withComponents.Add(typeof(T2));
        _withComponents.Add(typeof(T3));
        InvalidateCache();
        return this;
    }

    /// <summary>
    /// Exclude entities that have the specified component type
    /// </summary>
    public EntityQuery Without<T>() where T : Component
    {
        _withoutComponents.Add(typeof(T));
        InvalidateCache();
        return this;
    }

    /// <summary>
    /// Exclude entities that have the specified component types
    /// </summary>
    public EntityQuery Without<T1, T2>() 
        where T1 : Component 
        where T2 : Component
    {
        _withoutComponents.Add(typeof(T1));
        _withoutComponents.Add(typeof(T2));
        InvalidateCache();
        return this;
    }

    /// <summary>
    /// Include entities where the specified component matches a predicate
    /// </summary>
    public EntityQuery Where<T>(Func<T, bool> predicate) where T : Component
    {
        _withComponents.Add(typeof(T));
        _componentFilters[typeof(T)] = predicate;
        InvalidateCache();
        return this;
    }

    /// <summary>
    /// Enable caching for this query (results will be cached until world changes)
    /// </summary>
    public EntityQuery Cache()
    {
        _cached = true;
        return this;
    }

    /// <summary>
    /// Execute the query and return matching entities
    /// </summary>
    public IEnumerable<Entity> Execute()
    {
        // Return cached results if available
        if (_cached && _cachedResults != null)
        {
            return _cachedResults;
        }

        var results = ExecuteInternal();
        
        // Cache results if caching is enabled
        if (_cached)
        {
            _cachedResults = results.ToList();
            return _cachedResults;
        }

        return results;
    }

    /// <summary>
    /// Execute the query and return the first matching entity, or null
    /// </summary>
    public Entity? First()
    {
        return Execute().FirstOrDefault();
    }

    /// <summary>
    /// Execute the query and return a single entity (throws if not exactly one)
    /// </summary>
    public Entity Single()
    {
        var results = Execute().Take(2).ToList();
        
        if (results.Count == 0)
            throw new InvalidOperationException("No entities match the query");
        
        if (results.Count > 1)
            throw new InvalidOperationException("More than one entity matches the query");
            
        return results[0];
    }

    /// <summary>
    /// Execute the query and return entities with their specified component
    /// </summary>
    public IEnumerable<(Entity Entity, T Component)> ExecuteWithComponent<T>() where T : Component
    {
        foreach (var entity in Execute())
        {
            var component = _world.GetComponent<T>(entity.Id);
            if (component != null)
            {
                yield return (entity, component);
            }
        }
    }

    /// <summary>
    /// Execute the query and return entities with their two specified components
    /// </summary>
    public IEnumerable<(Entity Entity, T1 Component1, T2 Component2)> ExecuteWithComponents<T1, T2>() 
        where T1 : Component 
        where T2 : Component
    {
        foreach (var entity in Execute())
        {
            var component1 = _world.GetComponent<T1>(entity.Id);
            var component2 = _world.GetComponent<T2>(entity.Id);
            
            if (component1 != null && component2 != null)
            {
                yield return (entity, component1, component2);
            }
        }
    }

    /// <summary>
    /// Execute the query and return entities with their three specified components
    /// </summary>
    public IEnumerable<(Entity Entity, T1 Component1, T2 Component2, T3 Component3)> ExecuteWithComponents<T1, T2, T3>() 
        where T1 : Component 
        where T2 : Component 
        where T3 : Component
    {
        foreach (var entity in Execute())
        {
            var component1 = _world.GetComponent<T1>(entity.Id);
            var component2 = _world.GetComponent<T2>(entity.Id);
            var component3 = _world.GetComponent<T3>(entity.Id);
            
            if (component1 != null && component2 != null && component3 != null)
            {
                yield return (entity, component1, component2, component3);
            }
        }
    }

    /// <summary>
    /// Count the number of entities that match the query
    /// </summary>
    public int Count()
    {
        return Execute().Count();
    }

    /// <summary>
    /// Check if any entities match the query
    /// </summary>
    public bool Any()
    {
        return Execute().Any();
    }

    /// <summary>
    /// Convert to array
    /// </summary>
    public Entity[] ToArray()
    {
        return Execute().ToArray();
    }

    /// <summary>
    /// Convert to list
    /// </summary>
    public List<Entity> ToList()
    {
        return Execute().ToList();
    }

    private IEnumerable<Entity> ExecuteInternal()
    {
        // If no constraints, return all entities
        if (_withComponents.Count == 0 && _withoutComponents.Count == 0)
        {
            return _world.GetAllEntities();
        }

        IEnumerable<Entity> candidates = _world.GetAllEntities();

        // Apply "with" constraints
        foreach (var componentType in _withComponents)
        {
            candidates = FilterByComponentType(candidates, componentType, true);
        }

        // Apply "without" constraints
        foreach (var componentType in _withoutComponents)
        {
            candidates = FilterByComponentType(candidates, componentType, false);
        }

        // Apply component filters
        foreach (var (componentType, filter) in _componentFilters)
        {
            candidates = ApplyComponentFilter(candidates, componentType, filter);
        }

        return candidates;
    }

    private IEnumerable<Entity> FilterByComponentType(IEnumerable<Entity> entities, Type componentType, bool shouldHave)
    {
        return entities.Where(entity =>
        {
            var hasComponent = HasComponentOfType(entity, componentType);
            return shouldHave ? hasComponent : !hasComponent;
        });
    }

    private IEnumerable<Entity> ApplyComponentFilter(IEnumerable<Entity> entities, Type componentType, object filter)
    {
        return entities.Where(entity =>
        {
            var component = GetComponentOfType(entity, componentType);
            if (component == null)
                return false;

            // Use reflection to invoke the predicate
            var predicateMethod = filter.GetType().GetMethod("Invoke");
            if (predicateMethod != null)
            {
                var result = predicateMethod.Invoke(filter, new[] { component });
                return result is bool boolResult && boolResult;
            }

            return false;
        });
    }

    private bool HasComponentOfType(Entity entity, Type componentType)
    {
        var method = _world.GetType().GetMethod("HasComponent");
        if (method != null)
        {
            var genericMethod = method.MakeGenericMethod(componentType);
            var result = genericMethod.Invoke(_world, new object[] { entity.Id });
            return result is bool hasComponent && hasComponent;
        }

        return false;
    }

    private Component? GetComponentOfType(Entity entity, Type componentType)
    {
        var method = _world.GetType().GetMethod("GetComponent");
        if (method != null)
        {
            var genericMethod = method.MakeGenericMethod(componentType);
            var result = genericMethod.Invoke(_world, new object[] { entity.Id });
            return result as Component;
        }

        return null;
    }

    private void InvalidateCache()
    {
        _cachedResults = null;
    }

    public override string ToString()
    {
        var withTypes = _withComponents.Any() ? $"With: [{string.Join(", ", _withComponents.Select(t => t.Name))}]" : "";
        var withoutTypes = _withoutComponents.Any() ? $"Without: [{string.Join(", ", _withoutComponents.Select(t => t.Name))}]" : "";
        var filters = _componentFilters.Any() ? $"Filters: {_componentFilters.Count}" : "";

        var parts = new[] { withTypes, withoutTypes, filters }.Where(s => !string.IsNullOrEmpty(s));
        return $"EntityQuery({string.Join(", ", parts)})";
    }
}

/// <summary>
/// Extension methods for World to create queries
/// </summary>
public static class WorldQueryExtensions
{
    /// <summary>
    /// Create a new entity query
    /// </summary>
    public static EntityQuery Query(this World world)
    {
        return new EntityQuery(world);
    }
}