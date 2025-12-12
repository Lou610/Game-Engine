using System;
using System.Collections.Generic;
using Engine.Domain.ECS;
using Engine.Domain.ECS.Components;
using Engine.Domain.Rendering;
using Engine.Domain.Scene.Serialization;

namespace Engine.Domain.Scene.Prefabs;

/// <summary>
/// Serializable data structure for prefab persistence
/// </summary>
public class PrefabData
{
    /// <summary>
    /// Unique identifier for the prefab
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Human-readable name
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Optional description
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Parent prefab ID (null if root prefab)
    /// </summary>
    public Guid? ParentId { get; set; }
    
    /// <summary>
    /// Serialized component templates
    /// </summary>
    public List<SerializedComponent> Components { get; set; } = new();
    
    /// <summary>
    /// Child prefabs with their relative transforms
    /// </summary>
    public List<PrefabChildData> Children { get; set; } = new();
    
    /// <summary>
    /// Metadata dictionary
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();
    
    /// <summary>
    /// Version tracking
    /// </summary>
    public int Version { get; set; } = 1;
    
    /// <summary>
    /// Creation timestamp
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Last modification timestamp
    /// </summary>
    public DateTime ModifiedAt { get; set; }
    
    /// <summary>
    /// Tags for categorization
    /// </summary>
    public HashSet<string> Tags { get; set; } = new();
}

/// <summary>
/// Serializable data for child prefab relationships
/// </summary>
public class PrefabChildData
{
    /// <summary>
    /// ID of the child prefab
    /// </summary>
    public Guid PrefabId { get; set; }
    
    /// <summary>
    /// Relative transform from parent
    /// </summary>
    public TransformData RelativeTransform { get; set; } = new();
    
    /// <summary>
    /// Optional name override for this instance
    /// </summary>
    public string? NameOverride { get; set; }
}

/// <summary>
/// Collection of prefabs for batch operations
/// </summary>
public class PrefabCollection
{
    /// <summary>
    /// All prefabs in this collection
    /// </summary>
    public List<PrefabData> Prefabs { get; set; } = new();
    
    /// <summary>
    /// Collection metadata
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();
    
    /// <summary>
    /// Collection version
    /// </summary>
    public string Version { get; set; } = "1.0.0";
    
    /// <summary>
    /// When this collection was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Data for prefab instantiation requests
/// </summary>
public class PrefabInstantiationRequest
{
    /// <summary>
    /// ID of the prefab to instantiate
    /// </summary>
    public Guid PrefabId { get; set; }
    
    /// <summary>
    /// Target scene for instantiation  
    /// </summary>
    public Engine.Domain.Scene.Scene TargetScene { get; set; } = null!;
    
    /// <summary>
    /// Parent scene node (null for root)
    /// </summary>
    public SceneNode? ParentNode { get; set; }
    
    /// <summary>
    /// World transform for the instantiated entity
    /// </summary>
    public Transform? WorldTransform { get; set; }
    
    /// <summary>
    /// Component overrides to apply during instantiation
    /// </summary>
    public Dictionary<Type, object> ComponentOverrides { get; set; } = new();
    
    /// <summary>
    /// Custom name for the instantiated entity
    /// </summary>
    public string? CustomName { get; set; }
    
    /// <summary>
    /// Whether to instantiate child prefabs recursively
    /// </summary>
    public bool InstantiateChildren { get; set; } = true;
    
    /// <summary>
    /// Maximum depth for recursive instantiation (prevents infinite loops)
    /// </summary>
    public int MaxDepth { get; set; } = 10;
}

/// <summary>
/// Result of a prefab instantiation operation
/// </summary>
public class PrefabInstantiationResult
{
    /// <summary>
    /// The root entity that was created
    /// </summary>
    public Entity RootEntity { get; set; } = null!;
    
    /// <summary>
    /// All entities created during instantiation (including children)
    /// </summary>
    public List<Entity> CreatedEntities { get; set; } = new();
    
    /// <summary>
    /// Scene nodes created for the entities
    /// </summary>
    public List<SceneNode> CreatedNodes { get; set; } = new();
    
    /// <summary>
    /// Whether the instantiation was successful
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Error message if instantiation failed
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Additional metadata about the instantiation
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Statistics about prefab usage and performance
/// </summary>
public class PrefabStatistics
{
    /// <summary>
    /// Total number of prefabs loaded
    /// </summary>
    public int TotalPrefabs { get; set; }
    
    /// <summary>
    /// Number of instantiations performed
    /// </summary>
    public int TotalInstantiations { get; set; }
    
    /// <summary>
    /// Average instantiation time in milliseconds
    /// </summary>
    public double AverageInstantiationTime { get; set; }
    
    /// <summary>
    /// Memory usage for prefab templates
    /// </summary>
    public long MemoryUsageBytes { get; set; }
    
    /// <summary>
    /// Most frequently used prefabs
    /// </summary>
    public Dictionary<Guid, int> UsageCount { get; set; } = new();
}