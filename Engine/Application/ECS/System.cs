using System.Collections.Generic;
using System.Linq;
using Engine.Domain.ECS;

namespace Engine.Application.ECS;

/// <summary>
/// Application service processing components - enhanced with World access and queries
/// </summary>
public abstract class System
{
    /// <summary>
    /// The world this system operates on
    /// </summary>
    public World World { get; private set; }
    
    /// <summary>
    /// System execution priority (lower numbers execute first)
    /// </summary>
    public virtual int Priority => 0;
    
    /// <summary>
    /// Whether this system is currently enabled
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Initialize the system with a world reference
    /// </summary>
    public virtual void Initialize(World world) 
    { 
        World = world;
    }
    
    /// <summary>
    /// Update the system (called every frame)
    /// </summary>
    public virtual void Update(float deltaTime) { }
    
    /// <summary>
    /// Fixed update (called at fixed intervals)
    /// </summary>
    public virtual void FixedUpdate(float fixedDeltaTime) { }
    
    /// <summary>
    /// Shutdown the system
    /// </summary>
    public virtual void Shutdown() 
    { 
        World = null;
    }

    /// <summary>
    /// Query entities that have a specific component
    /// </summary>
    protected IEnumerable<Entity> Query<T>() where T : Component
    {
        return World?.Query<T>() ?? Enumerable.Empty<Entity>();
    }

    /// <summary>
    /// Query entities that have two specific components
    /// </summary>
    protected IEnumerable<Entity> Query<T1, T2>() 
        where T1 : Component 
        where T2 : Component
    {
        return World?.Query<T1, T2>() ?? Enumerable.Empty<Entity>();
    }

    /// <summary>
    /// Query entities that have three specific components
    /// </summary>
    protected IEnumerable<Entity> Query<T1, T2, T3>() 
        where T1 : Component 
        where T2 : Component 
        where T3 : Component
    {
        return World?.Query<T1, T2, T3>() ?? Enumerable.Empty<Entity>();
    }

    /// <summary>
    /// Get all entities with their component of a specific type
    /// </summary>
    protected IEnumerable<(Entity Entity, T Component)> GetEntitiesWithComponent<T>() where T : Component
    {
        return World?.GetEntitiesWithComponent<T>() ?? Enumerable.Empty<(Entity, T)>();
    }

    /// <summary>
    /// Get all components of a specific type
    /// </summary>
    protected IEnumerable<T> GetAllComponents<T>() where T : Component
    {
        return World?.GetAllComponents<T>() ?? Enumerable.Empty<T>();
    }
}

