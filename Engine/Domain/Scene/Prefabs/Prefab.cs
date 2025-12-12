using System;
using System.Collections.Generic;
using System.Linq;
using Engine.Domain.ECS;
using Engine.Domain.ECS.Components;
using Engine.Domain.Rendering;
using Engine.Domain.Scene.ValueObjects;

namespace Engine.Domain.Scene.Prefabs;

/// <summary>
/// Core domain entity representing a reusable entity template
/// </summary>
public class Prefab
{
    /// <summary>
    /// Unique identifier for this prefab
    /// </summary>
    public PrefabId Id { get; private set; }
    
    /// <summary>
    /// Human-readable name for this prefab
    /// </summary>
    public string Name { get; private set; }
    
    /// <summary>
    /// Optional description of the prefab's purpose
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Parent prefab for inheritance (null if root prefab)
    /// </summary>
    public Prefab? Parent { get; private set; }
    
    /// <summary>
    /// Template components that will be applied to instantiated entities
    /// </summary>
    public IReadOnlyDictionary<Type, Component> ComponentTemplates => _componentTemplates;
    private readonly Dictionary<Type, Component> _componentTemplates = new();
    
    /// <summary>
    /// Child prefabs for hierarchical structures
    /// </summary>
    public IReadOnlyList<PrefabChild> Children => _children;
    private readonly List<PrefabChild> _children = new();
    
    /// <summary>
    /// Metadata for this prefab
    /// </summary>
    public IReadOnlyDictionary<string, object> Metadata => _metadata;
    private readonly Dictionary<string, object> _metadata = new();
    
    /// <summary>
    /// Version tracking for prefab changes
    /// </summary>
    public int Version { get; private set; } = 1;
    
    /// <summary>
    /// When this prefab was created
    /// </summary>
    public DateTime CreatedAt { get; private set; }
    
    /// <summary>
    /// When this prefab was last modified
    /// </summary>
    public DateTime ModifiedAt { get; private set; }
    
    /// <summary>
    /// Tags for categorization and filtering
    /// </summary>
    public IReadOnlySet<string> Tags => _tags;
    private readonly HashSet<string> _tags = new();

    public Prefab(PrefabId id, string name, Prefab? parent = null)
    {
        if (id.Value == null)
            throw new ArgumentNullException(nameof(id));
        Id = id;
        Name = !string.IsNullOrWhiteSpace(name) ? name : throw new ArgumentException("Name cannot be null or empty", nameof(name));
        Parent = parent;
        CreatedAt = DateTime.UtcNow;
        ModifiedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Add or update a component template
    /// </summary>
    public void SetComponent<T>(T component) where T : Component
    {
        if (component == null)
            throw new ArgumentNullException(nameof(component));
            
        _componentTemplates[typeof(T)] = component;
        MarkModified();
    }

    /// <summary>
    /// Remove a component template
    /// </summary>
    public bool RemoveComponent<T>() where T : Component
    {
        var removed = _componentTemplates.Remove(typeof(T));
        if (removed)
            MarkModified();
        return removed;
    }

    /// <summary>
    /// Get a component template of the specified type
    /// </summary>
    public T? GetComponent<T>() where T : Component
    {
        return _componentTemplates.TryGetValue(typeof(T), out var component) ? component as T : null;
    }

    /// <summary>
    /// Check if this prefab has a component template of the specified type
    /// </summary>
    public bool HasComponent<T>() where T : Component
    {
        return _componentTemplates.ContainsKey(typeof(T));
    }

    /// <summary>
    /// Add a child prefab
    /// </summary>
    public void AddChild(Prefab childPrefab, Transform? relativeTransform = null)
    {
        if (childPrefab == null)
            throw new ArgumentNullException(nameof(childPrefab));
            
        if (childPrefab == this)
            throw new ArgumentException("Cannot add self as child");
            
        // Check for circular references
        if (IsDescendantOf(childPrefab))
            throw new ArgumentException("Cannot add ancestor as child - would create circular reference");
            
        var child = new PrefabChild(childPrefab, relativeTransform ?? Transform.Identity);
        _children.Add(child);
        MarkModified();
    }

    /// <summary>
    /// Remove a child prefab
    /// </summary>
    public bool RemoveChild(Prefab childPrefab)
    {
        var removed = _children.RemoveAll(c => c.Prefab == childPrefab) > 0;
        if (removed)
            MarkModified();
        return removed;
    }

    /// <summary>
    /// Add metadata
    /// </summary>
    public void SetMetadata(string key, object value)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Metadata key cannot be null or empty", nameof(key));
            
        _metadata[key] = value;
        MarkModified();
    }

    /// <summary>
    /// Add a tag
    /// </summary>
    public void AddTag(string tag)
    {
        if (!string.IsNullOrWhiteSpace(tag))
        {
            _tags.Add(tag.Trim().ToLowerInvariant());
            MarkModified();
        }
    }

    /// <summary>
    /// Remove a tag
    /// </summary>
    public bool RemoveTag(string tag)
    {
        if (string.IsNullOrWhiteSpace(tag))
            return false;
            
        var removed = _tags.Remove(tag.Trim().ToLowerInvariant());
        if (removed)
            MarkModified();
        return removed;
    }

    /// <summary>
    /// Get all components including inherited ones from parent prefabs
    /// </summary>
    public Dictionary<Type, Component> GetAllComponents()
    {
        var allComponents = new Dictionary<Type, Component>();
        
        // Start with parent components (if any)
        if (Parent != null)
        {
            var parentComponents = Parent.GetAllComponents();
            foreach (var kvp in parentComponents)
            {
                allComponents[kvp.Key] = kvp.Value;
            }
        }
        
        // Override with our own components
        foreach (var kvp in _componentTemplates)
        {
            allComponents[kvp.Key] = kvp.Value;
        }
        
        return allComponents;
    }

    /// <summary>
    /// Check if this prefab is a descendant of the specified prefab
    /// </summary>
    private bool IsDescendantOf(Prefab potentialAncestor)
    {
        var current = Parent;
        while (current != null)
        {
            if (current == potentialAncestor)
                return true;
            current = current.Parent;
        }
        return false;
    }

    private void MarkModified()
    {
        ModifiedAt = DateTime.UtcNow;
        Version++;
    }

    public override string ToString()
    {
        return $"Prefab '{Name}' (ID: {Id}, Components: {_componentTemplates.Count}, Children: {_children.Count})";
    }
}

/// <summary>
/// Represents a child prefab within a parent prefab hierarchy
/// </summary>
public class PrefabChild
{
    /// <summary>
    /// The child prefab
    /// </summary>
    public Prefab Prefab { get; }
    
    /// <summary>
    /// Relative transform from parent to this child
    /// </summary>
    public Transform RelativeTransform { get; set; }
    
    /// <summary>
    /// Optional name override for this child instance
    /// </summary>
    public string? NameOverride { get; set; }

    public PrefabChild(Prefab prefab, Transform relativeTransform)
    {
        Prefab = prefab ?? throw new ArgumentNullException(nameof(prefab));
        RelativeTransform = relativeTransform;
    }
}

