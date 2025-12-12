using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Engine.Domain.ECS;
using Engine.Domain.ECS.ValueObjects;
using Engine.Domain.ECS.Components;

namespace Engine.Domain.Scene;

/// <summary>
/// Hierarchical scene graph managing entity parent-child relationships and transforms
/// </summary>
public class SceneGraph : IDisposable
{
    private readonly Dictionary<EntityId, SceneNode> _entityToNode;
    private readonly Dictionary<string, SceneNode> _nameToNode;
    private Scene? _scene;
    private bool _disposed;

    /// <summary>
    /// Root node of the scene graph
    /// </summary>
    public SceneNode Root { get; private set; }

    /// <summary>
    /// Total number of nodes in the graph
    /// </summary>
    public int NodeCount => _entityToNode.Count;

    public SceneGraph()
    {
        _entityToNode = new Dictionary<EntityId, SceneNode>();
        _nameToNode = new Dictionary<string, SceneNode>();
        
        // Create root node with invalid EntityId
        Root = new SceneNode(default, null, "Root")
        {
            LocalTransform = Transform.Identity,
            IsRoot = true
        };
    }

    /// <summary>
    /// Initialize the scene graph with a parent scene
    /// </summary>
    public void Initialize(Scene scene)
    {
        _scene = scene ?? throw new ArgumentNullException(nameof(scene));
    }

    /// <summary>
    /// Create a new scene node for an entity
    /// </summary>
    public SceneNode CreateNode(EntityId entityId, SceneNode? parent = null, string name = "Node")
    {
        if (_entityToNode.ContainsKey(entityId))
            throw new InvalidOperationException($"Entity {entityId} already has a scene node");

        // Use root as parent if none specified
        parent ??= Root;

        var node = new SceneNode(entityId, parent, name);
        
        // Add to lookup tables
        _entityToNode[entityId] = node;
        UpdateNameIndex(node);

        // Add to parent's children
        parent.AddChild(node);

        // Initialize transform from entity if it has one
        InitializeNodeTransform(node);

        return node;
    }

    /// <summary>
    /// Remove a node and all its children from the graph
    /// </summary>
    public void RemoveNode(SceneNode node)
    {
        if (node == null || node.IsRoot)
            return;

        // Recursively remove all children first
        var children = node.Children.ToList();
        foreach (var child in children)
        {
            RemoveNode(child);
        }

        // Remove from parent
        node.Parent?.RemoveChild(node);

        // Remove from lookup tables
        _entityToNode.Remove(node.EntityId);
        RemoveFromNameIndex(node);

        // Dispose the node
        node.Dispose();
    }

    /// <summary>
    /// Find node by entity ID
    /// </summary>
    public SceneNode? FindNode(EntityId entityId)
    {
        return _entityToNode.TryGetValue(entityId, out var node) ? node : null;
    }

    /// <summary>
    /// Find node by name (returns first match)
    /// </summary>
    public SceneNode? FindNodeByName(string name)
    {
        return _nameToNode.TryGetValue(name, out var node) ? node : null;
    }

    /// <summary>
    /// Find all nodes with the specified name
    /// </summary>
    public IEnumerable<SceneNode> FindNodesByName(string name)
    {
        return _entityToNode.Values.Where(node => node.Name == name);
    }

    /// <summary>
    /// Get all nodes in the graph (excluding root)
    /// </summary>
    public IEnumerable<SceneNode> GetAllNodes()
    {
        return _entityToNode.Values;
    }

    /// <summary>
    /// Get all root-level nodes (direct children of root)
    /// </summary>
    public IEnumerable<SceneNode> GetRootNodes()
    {
        return Root.Children;
    }

    /// <summary>
    /// Update world transforms for all nodes (called per frame)
    /// </summary>
    public void UpdateTransforms()
    {
        UpdateNodeTransformsRecursive(Root);
    }

    /// <summary>
    /// Reparent a node to a new parent
    /// </summary>
    public void ReparentNode(SceneNode node, SceneNode? newParent)
    {
        if (node == null || node.IsRoot)
            return;

        newParent ??= Root;

        // Remove from current parent
        node.Parent?.RemoveChild(node);

        // Add to new parent
        newParent.AddChild(node);
        node.SetParent(newParent);

        // Update transforms
        UpdateNodeTransformsRecursive(node);
    }

    /// <summary>
    /// Get the path from root to the specified node
    /// </summary>
    public string GetNodePath(SceneNode node)
    {
        if (node == null || node.IsRoot)
            return "/";

        var path = new List<string>();
        var current = node;

        while (current != null && !current.IsRoot)
        {
            path.Insert(0, current.Name);
            current = current.Parent;
        }

        return "/" + string.Join("/", path);
    }

    /// <summary>
    /// Find node by path (e.g., "/Player/Weapon/Barrel")
    /// </summary>
    public SceneNode? FindNodeByPath(string path)
    {
        if (string.IsNullOrEmpty(path) || path == "/")
            return Root;

        var parts = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var current = Root;

        foreach (var part in parts)
        {
            current = current.Children.FirstOrDefault(child => child.Name == part);
            if (current == null)
                return null;
        }

        return current;
    }

    /// <summary>
    /// Update name in the index when node name changes
    /// </summary>
    internal void OnNodeNameChanged(SceneNode node, string oldName)
    {
        if (!string.IsNullOrEmpty(oldName))
        {
            _nameToNode.Remove(oldName);
        }
        
        UpdateNameIndex(node);
    }

    private void InitializeNodeTransform(SceneNode node)
    {
        if (_scene?.Entities == null)
            return;

        // Try to get Transform component from entity
        if (_scene.Entities.HasComponent<Transform>(node.EntityId))
        {
            var transform = _scene.Entities.GetComponent<Transform>(node.EntityId);
            node.LocalTransform = transform ?? Transform.Identity;
        }
        else
        {
            // Add default transform component
            var defaultTransform = Transform.Identity;
            _scene.Entities.AddComponent(node.EntityId, defaultTransform);
            node.LocalTransform = defaultTransform;
        }
    }

    private void UpdateNodeTransformsRecursive(SceneNode node)
    {
        // Calculate world transform
        if (node.Parent != null && !node.IsRoot)
        {
            node.WorldTransform = Transform.Combine(node.Parent.WorldTransform, node.LocalTransform);
        }
        else
        {
            node.WorldTransform = node.LocalTransform;
        }

        // Sync with ECS Transform component if scene is available
        if (_scene?.Entities != null && node.EntityId != default && 
            _scene.Entities.HasComponent<Transform>(node.EntityId))
        {
            _scene.Entities.AddComponent(node.EntityId, node.WorldTransform);
        }

        // Update all children
        foreach (var child in node.Children)
        {
            UpdateNodeTransformsRecursive(child);
        }
    }

    private void UpdateNameIndex(SceneNode node)
    {
        if (!string.IsNullOrEmpty(node.Name))
        {
            _nameToNode[node.Name] = node;
        }
    }

    private void RemoveFromNameIndex(SceneNode node)
    {
        if (!string.IsNullOrEmpty(node.Name))
        {
            _nameToNode.Remove(node.Name);
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            // Remove all nodes
            var allNodes = _entityToNode.Values.ToList();
            foreach (var node in allNodes)
            {
                RemoveNode(node);
            }

            // Clear collections
            _entityToNode.Clear();
            _nameToNode.Clear();

            // Dispose root
            Root?.Dispose();

            _disposed = true;
        }
    }
}