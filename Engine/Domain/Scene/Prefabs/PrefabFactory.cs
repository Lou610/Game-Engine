using System;
using System.Collections.Generic;
using System.Linq;
using Engine.Domain.ECS;
using Engine.Domain.ECS.Components;
using Engine.Domain.Rendering;
using Engine.Domain.Scene.ValueObjects;
using Engine.Infrastructure.Logging;

namespace Engine.Domain.Scene.Prefabs;

/// <summary>
/// Factory for creating and managing prefabs
/// </summary>
public class PrefabFactory
{
    private readonly Logger _logger;
    private readonly Dictionary<PrefabId, Prefab> _prefabCache = new();
    private readonly Dictionary<string, PrefabId> _nameToIdMap = new();
    private PrefabStatistics _statistics = new();

    public PrefabFactory(Logger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        InitializeBuiltInPrefabs();
    }

    /// <summary>
    /// Create a new prefab from scratch
    /// </summary>
    public Prefab CreatePrefab(string name, Prefab? parent = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Prefab name cannot be null or empty", nameof(name));

        if (_nameToIdMap.ContainsKey(name))
            throw new ArgumentException($"Prefab with name '{name}' already exists", nameof(name));

        var id = new PrefabId(Guid.NewGuid().ToString());
        var prefab = new Prefab(id, name, parent);
        
        RegisterPrefab(prefab);
        _logger.Debug($"Created new prefab: {prefab}");
        
        return prefab;
    }

    /// <summary>
    /// Create a prefab from an existing entity
    /// </summary>
    public Prefab CreatePrefabFromEntity(Entity entity, string name, Engine.Domain.Scene.Scene scene)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Prefab name cannot be null or empty", nameof(name));
        if (scene == null)
            throw new ArgumentNullException(nameof(scene));

        var prefab = CreatePrefab(name);
        
        // Copy all components from the entity - simplified for now
        // In a full implementation, this would iterate through entity components
        // For now, we'll add a default Transform component
        var defaultTransform = Transform.Identity;
        prefab.SetComponent(defaultTransform);
        
        // Add metadata
        prefab.SetMetadata("SourceEntityId", entity.Id.ToString());
        prefab.SetMetadata("CreatedFromEntity", true);
        
        _logger.Info($"Created prefab '{name}' from entity {entity.Id}");
        return prefab;
    }

    /// <summary>
    /// Create a prefab with hierarchical structure from a scene node and its children
    /// </summary>
    public Prefab CreatePrefabFromSceneNode(SceneNode node, string name, Engine.Domain.Scene.Scene scene)
    {
        if (node == null)
            throw new ArgumentNullException(nameof(node));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Prefab name cannot be null or empty", nameof(name));
        if (scene == null)
            throw new ArgumentNullException(nameof(scene));

        // Create the root prefab from the node's entity
        var rootEntity = scene.Entities.GetEntity(node.EntityId);
        if (rootEntity == null)
            throw new InvalidOperationException($"Entity with ID {node.EntityId} not found in scene");
        var prefab = CreatePrefabFromEntity(rootEntity, name, scene);
        
        // Recursively add child prefabs
        foreach (var childNode in node.Children)
        {
            var childEntity = scene.Entities.GetEntity(childNode.EntityId);
            if (childEntity == null) continue;
            var childName = $"{name}_Child_{childEntity.Name}";
            var childPrefab = CreatePrefabFromSceneNode(childNode, childName, scene);
            
            prefab.AddChild(childPrefab, childNode.LocalTransform);
        }
        
        prefab.SetMetadata("CreatedFromSceneNode", true);
        prefab.SetMetadata("SourceNodeDepth", CountNodeDepth(node));
        
        return prefab;
    }

    /// <summary>
    /// Get a prefab by ID
    /// </summary>
    public Prefab? GetPrefab(PrefabId id)
    {
        return _prefabCache.TryGetValue(id, out var prefab) ? prefab : null;
    }

    /// <summary>
    /// Get a prefab by name
    /// </summary>
    public Prefab? GetPrefab(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return null;
            
        return _nameToIdMap.TryGetValue(name, out var id) ? GetPrefab(id) : null;
    }

    /// <summary>
    /// Register an existing prefab
    /// </summary>
    public void RegisterPrefab(Prefab prefab)
    {
        if (prefab == null)
            throw new ArgumentNullException(nameof(prefab));

        _prefabCache[prefab.Id] = prefab;
        _nameToIdMap[prefab.Name] = prefab.Id;
        _statistics.TotalPrefabs = _prefabCache.Count;
    }

    /// <summary>
    /// Remove a prefab from the factory
    /// </summary>
    public bool RemovePrefab(PrefabId id)
    {
        if (_prefabCache.TryGetValue(id, out var prefab))
        {
            _prefabCache.Remove(id);
            _nameToIdMap.Remove(prefab.Name);
            _statistics.TotalPrefabs = _prefabCache.Count;
            
            _logger.Info($"Removed prefab: {prefab}");
            return true;
        }
        return false;
    }

    /// <summary>
    /// Get all registered prefabs
    /// </summary>
    public IReadOnlyCollection<Prefab> GetAllPrefabs()
    {
        return _prefabCache.Values.ToList();
    }

    /// <summary>
    /// Find prefabs by tag
    /// </summary>
    public IEnumerable<Prefab> FindPrefabsByTag(string tag)
    {
        if (string.IsNullOrWhiteSpace(tag))
            return Enumerable.Empty<Prefab>();
            
        var normalizedTag = tag.Trim().ToLowerInvariant();
        return _prefabCache.Values.Where(p => p.Tags.Contains(normalizedTag));
    }

    /// <summary>
    /// Find prefabs that have a specific component type
    /// </summary>
    public IEnumerable<Prefab> FindPrefabsWithComponent<T>() where T : Component
    {
        return _prefabCache.Values.Where(p => p.HasComponent<T>());
    }

    /// <summary>
    /// Get prefab statistics
    /// </summary>
    public PrefabStatistics GetStatistics()
    {
        return _statistics;
    }

    /// <summary>
    /// Clear all prefabs (except built-in ones)
    /// </summary>
    public void Clear(bool includeBuiltIns = false)
    {
        if (includeBuiltIns)
        {
            _prefabCache.Clear();
            _nameToIdMap.Clear();
            InitializeBuiltInPrefabs();
        }
        else
        {
            var builtInPrefabs = _prefabCache.Values
                .Where(p => p.Tags.Contains("builtin"))
                .ToList();
                
            _prefabCache.Clear();
            _nameToIdMap.Clear();
            
            foreach (var builtIn in builtInPrefabs)
            {
                RegisterPrefab(builtIn);
            }
        }
        
        _statistics.TotalPrefabs = _prefabCache.Count;
        _logger.Info($"Cleared prefab factory. Remaining prefabs: {_prefabCache.Count}");
    }

    /// <summary>
    /// Create built-in primitive prefabs
    /// </summary>
    private void InitializeBuiltInPrefabs()
    {
        CreateCubePrefab();
        CreateSpherePrefab();
        CreatePlanePrefab();
        CreateCameraPrefab();
        CreateLightPrefab();
        
        _logger.Debug("Initialized built-in prefabs");
    }

    private void CreateCubePrefab()
    {
        var cube = new Prefab(new PrefabId(Guid.NewGuid().ToString()), "Cube");
        cube.SetComponent(Transform.Identity);
        // Note: Mesh and MeshRenderer components would be added here when available
        cube.AddTag("primitive");
        cube.AddTag("builtin");
        cube.SetMetadata("PrimitiveType", "Cube");
        
        RegisterPrefab(cube);
    }

    private void CreateSpherePrefab()
    {
        var sphere = new Prefab(new PrefabId(Guid.NewGuid().ToString()), "Sphere");
        sphere.SetComponent(Transform.Identity);
        sphere.AddTag("primitive");
        sphere.AddTag("builtin");
        sphere.SetMetadata("PrimitiveType", "Sphere");
        
        RegisterPrefab(sphere);
    }

    private void CreatePlanePrefab()
    {
        var plane = new Prefab(new PrefabId(Guid.NewGuid().ToString()), "Plane");
        plane.SetComponent(Transform.Identity);
        plane.AddTag("primitive");
        plane.AddTag("builtin");
        plane.SetMetadata("PrimitiveType", "Plane");
        
        RegisterPrefab(plane);
    }

    private void CreateCameraPrefab()
    {
        var camera = new Prefab(new PrefabId(Guid.NewGuid().ToString()), "Camera");
        camera.SetComponent(Transform.Identity);
        // Note: Camera component would be added here when available
        camera.AddTag("camera");
        camera.AddTag("builtin");
        
        RegisterPrefab(camera);
    }

    private void CreateLightPrefab()
    {
        var light = new Prefab(new PrefabId(Guid.NewGuid().ToString()), "Light");
        light.SetComponent(Transform.Identity);
        // Note: Light component would be added here when available
        light.AddTag("light");
        light.AddTag("builtin");
        
        RegisterPrefab(light);
    }

    /// <summary>
    /// Clone a component for prefab template storage
    /// </summary>
    private Component CloneComponent(Component original)
    {
        // For now, we'll do a shallow copy
        // In a full implementation, this would use proper deep cloning or serialization
        return original switch
        {
            Transform transform => new Transform
            {
                Position = transform.Position,
                Rotation = transform.Rotation,
                Scale = transform.Scale
            },
            _ => original // Fallback - in practice, all components should be cloneable
        };
    }

    /// <summary>
    /// Count the depth of a scene node hierarchy
    /// </summary>
    private int CountNodeDepth(SceneNode node)
    {
        if (!node.Children.Any())
            return 1;
            
        return 1 + node.Children.Max(CountNodeDepth);
    }
}