using System;
using System.Collections.Generic;
using System.Linq;
using Engine.Domain.Core;
using Engine.Domain.ECS;
using Engine.Domain.ECS.ValueObjects;
using Engine.Domain.Scene.ValueObjects;

namespace Engine.Domain.Scene;

/// <summary>
/// Core scene entity that manages scene state and hierarchy
/// </summary>
public class Scene : Entity
{
    private readonly Dictionary<string, object> _metadata;
    private bool _isActive;
    private bool _isLoaded;

    /// <summary>
    /// Scene identifier
    /// </summary>
    public new SceneId Id { get; set; }

    /// <summary>
    /// Display name of the scene
    /// </summary>
    public new string Name { get; set; }

    /// <summary>
    /// Scene graph managing entity hierarchy
    /// </summary>
    public SceneGraph SceneGraph { get; private set; }

    /// <summary>
    /// ECS world containing all scene entities
    /// </summary>
    public World Entities { get; private set; }

    /// <summary>
    /// Scene configuration and settings
    /// </summary>
    public SceneSettings Settings { get; set; }

    /// <summary>
    /// Scene metadata (tags, description, etc.)
    /// </summary>
    public IReadOnlyDictionary<string, object> Metadata => _metadata;

    /// <summary>
    /// Whether the scene is currently active
    /// </summary>
    public bool IsActive
    {
        get => _isActive;
        set
        {
            if (_isActive != value)
            {
                _isActive = value;
                if (_isActive)
                {
                    OnActivated();
                }
                else
                {
                    OnDeactivated();
                }
            }
        }
    }

    /// <summary>
    /// Whether the scene is loaded in memory
    /// </summary>
    public bool IsLoaded
    {
        get => _isLoaded;
        private set
        {
            if (_isLoaded != value)
            {
                _isLoaded = value;
                if (_isLoaded)
                {
                    OnLoaded();
                }
                else
                {
                    OnUnloaded();
                }
            }
        }
    }

    /// <summary>
    /// Scene creation timestamp
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Last modification timestamp
    /// </summary>
    public DateTime ModifiedAt { get; private set; }

    /// <summary>
    /// Scene version for serialization compatibility
    /// </summary>
    public string Version { get; set; } = "1.0";

    // Events
    public event EventHandler<SceneEventArgs>? EntityAdded;
    public event EventHandler<SceneEventArgs>? EntityRemoved;
    public event EventHandler<EventArgs>? SceneActivated;
    public event EventHandler<EventArgs>? SceneDeactivated;

    public Scene(SceneId id, string name) : base(new EntityId((ulong)id.Value.GetHashCode()), name)
    {
        Id = id;
        Name = name ?? throw new ArgumentNullException(nameof(name));
        _metadata = new Dictionary<string, object>();
        SceneGraph = new SceneGraph();
        Entities = new World();
        Settings = new SceneSettings();
        CreatedAt = DateTime.UtcNow;
        ModifiedAt = DateTime.UtcNow;
    }

    // Backward compatibility constructor
    public Scene(SceneId id, string name, World world) : this(id, name)
    {
        Entities = world ?? new World();
    }

    // Constructor for SceneManager
    public Scene(string name, World world, SceneSettings settings) : this(new SceneId(name), name, world)
    {
        Settings = settings ?? new SceneSettings();
    }

    /// <summary>
    /// Initialize the scene and prepare for use
    /// </summary>
    public void Initialize()
    {
        if (IsLoaded)
            return;

        try
        {
            // Initialize scene graph
            SceneGraph.Initialize(this);

            // ECS world is already initialized

            // Setup default systems if needed
            SetupDefaultSystems();

            IsLoaded = true;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to initialize scene '{Name}': {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Update the scene (typically called per frame)
    /// </summary>
    public void Update(float deltaTime)
    {
        if (!IsLoaded || !IsActive)
            return;

        try
        {
            // Update scene graph transforms
            SceneGraph.UpdateTransforms();

            // ECS world is updated by systems elsewhere

            // Update last modified time
            ModifiedAt = DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to update scene '{Name}': {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Create a new entity in this scene
    /// </summary>
    public EntityId CreateEntity(string name = "Entity")
    {
        if (!IsLoaded)
            throw new InvalidOperationException("Cannot create entity in unloaded scene");

        var entity = Entities.CreateEntity(name);
        
        // Create scene node for the entity
        var sceneNode = SceneGraph.CreateNode(entity.Id, null, name);

        EntityAdded?.Invoke(this, new SceneEventArgs(entity.Id, sceneNode));
        ModifiedAt = DateTime.UtcNow;

        return entity.Id;
    }

    /// <summary>
    /// Create entity as child of existing entity
    /// </summary>
    public EntityId CreateChildEntity(EntityId parentId, string name = "Entity")
    {
        if (!IsLoaded)
            throw new InvalidOperationException("Cannot create entity in unloaded scene");

        var parentNode = SceneGraph.FindNode(parentId);
        if (parentNode == null)
            throw new ArgumentException($"Parent entity {parentId} not found in scene");

        var entity = Entities.CreateEntity(name);
        var sceneNode = SceneGraph.CreateNode(entity.Id, parentNode, name);

        EntityAdded?.Invoke(this, new SceneEventArgs(entity.Id, sceneNode));
        ModifiedAt = DateTime.UtcNow;

        return entity.Id;
    }

    /// <summary>
    /// Remove entity from the scene
    /// </summary>
    public void RemoveEntity(EntityId entityId)
    {
        if (!IsLoaded)
            throw new InvalidOperationException("Cannot remove entity from unloaded scene");

        var sceneNode = SceneGraph.FindNode(entityId);
        if (sceneNode == null)
            return; // Entity not in scene

        // Remove from scene graph (also removes children)
        SceneGraph.RemoveNode(sceneNode);

        // Remove from ECS world
        Entities.DestroyEntity(entityId);

        EntityRemoved?.Invoke(this, new SceneEventArgs(entityId, sceneNode));
        ModifiedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Legacy method for backward compatibility
    /// </summary>
    public void AddEntity(Entity entity)
    {
        // Legacy method - convert to new approach if needed
        var newEntity = Entities.CreateEntity(entity.Name);
        SceneGraph.CreateNode(newEntity.Id, null, "LegacyEntity");
    }

    /// <summary>
    /// Legacy method for backward compatibility
    /// </summary>
    public void RemoveEntity(Entity entity)
    {
        // Legacy method - would need entity to EntityId mapping
    }

    /// <summary>
    /// Legacy method for backward compatibility
    /// </summary>
    public IEnumerable<Entity> GetEntities()
    {
        // Return empty for now - would need Entity wrapper around EntityId
        return Enumerable.Empty<Entity>();
    }

    /// <summary>
    /// Find entity by name in the scene
    /// </summary>
    public EntityId? FindEntityByName(string name)
    {
        var node = SceneGraph.FindNodeByName(name);
        return node?.EntityId;
    }

    /// <summary>
    /// Get all root entities in the scene
    /// </summary>
    public IEnumerable<EntityId> GetRootEntities()
    {
        return SceneGraph.Root.Children.Select(node => node.EntityId);
    }

    /// <summary>
    /// Get all entities in the scene (including children)
    /// </summary>
    public IEnumerable<EntityId> GetAllEntities()
    {
        return SceneGraph.GetAllNodes().Select(node => node.EntityId);
    }

    /// <summary>
    /// Set scene metadata
    /// </summary>
    public void SetMetadata(string key, object value)
    {
        _metadata[key] = value;
        ModifiedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Get scene metadata
    /// </summary>
    public T? GetMetadata<T>(string key) where T : class
    {
        return _metadata.TryGetValue(key, out var value) ? value as T : null;
    }

    /// <summary>
    /// Dispose scene and cleanup resources
    /// </summary>
    public void Dispose()
    {
        if (IsLoaded)
        {
            IsActive = false;
            
            // Cleanup scene graph
            SceneGraph?.Dispose();

            // Cleanup ECS world
            Entities?.Dispose();

            IsLoaded = false;
        }
    }

    private void SetupDefaultSystems()
    {
        // Setup basic systems for scene management
        // This would typically include transform systems, rendering systems, etc.
        // For Phase 4.2, we'll keep this minimal
    }

    private void OnLoaded()
    {
        // Scene loaded event handling
    }

    private void OnUnloaded()
    {
        // Scene unloaded event handling
    }

    private void OnActivated()
    {
        SceneActivated?.Invoke(this, EventArgs.Empty);
    }

    private void OnDeactivated()
    {
        SceneDeactivated?.Invoke(this, EventArgs.Empty);
    }
}

/// <summary>
/// Scene event arguments
/// </summary>
public class SceneEventArgs : EventArgs
{
    public EntityId EntityId { get; }
    public SceneNode SceneNode { get; }

    public SceneEventArgs(EntityId entityId, SceneNode sceneNode)
    {
        EntityId = entityId;
        SceneNode = sceneNode;
    }
}

