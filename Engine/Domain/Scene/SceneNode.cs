using System;
using System.Collections.Generic;
using System.Linq;
using Engine.Domain.ECS;
using Engine.Domain.ECS.ValueObjects;
using Engine.Domain.ECS.Components;

namespace Engine.Domain.Scene;

/// <summary>
/// Node in the scene graph representing an entity's hierarchical position and transform
/// </summary>
public class SceneNode : IDisposable
{
    private readonly List<SceneNode> _children;
    private SceneNode? _parent;
    private string _name;
    private Transform _localTransform;
    private Transform _worldTransform;
    private bool _disposed;

    /// <summary>
    /// Entity ID this node represents
    /// </summary>
    public EntityId EntityId { get; }

    /// <summary>
    /// Display name of the node
    /// </summary>
    public string Name
    {
        get => _name;
        set
        {
            if (_name != value)
            {
                var oldName = _name;
                _name = value;
                OnNameChanged(oldName);
            }
        }
    }

    /// <summary>
    /// Parent node in the hierarchy
    /// </summary>
    public SceneNode? Parent
    {
        get => _parent;
        private set => _parent = value;
    }

    /// <summary>
    /// Child nodes
    /// </summary>
    public IReadOnlyList<SceneNode> Children => _children;

    /// <summary>
    /// Local transform relative to parent
    /// </summary>
    public Transform LocalTransform
    {
        get => _localTransform;
        set
        {
            if (_localTransform != value)
            {
                _localTransform = value;
                OnTransformChanged();
            }
        }
    }

    /// <summary>
    /// World transform (computed from hierarchy)
    /// </summary>
    public Transform WorldTransform
    {
        get => _worldTransform;
        internal set => _worldTransform = value;
    }

    /// <summary>
    /// Whether this is the root node
    /// </summary>
    public bool IsRoot { get; internal set; }

    /// <summary>
    /// Whether this node is active in the hierarchy
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Whether this node is visible (affects rendering)
    /// </summary>
    public bool IsVisible { get; set; } = true;

    /// <summary>
    /// Depth in the hierarchy (root = 0)
    /// </summary>
    public int Depth
    {
        get
        {
            int depth = 0;
            var current = Parent;
            while (current != null && !current.IsRoot)
            {
                depth++;
                current = current.Parent;
            }
            return depth;
        }
    }

    /// <summary>
    /// Whether this node has any children
    /// </summary>
    public bool HasChildren => _children.Count > 0;

    /// <summary>
    /// Number of child nodes
    /// </summary>
    public int ChildCount => _children.Count;

    // Events
    public event EventHandler<TransformChangedEventArgs>? TransformChanged;
    public event EventHandler<NodeEventArgs>? ChildAdded;
    public event EventHandler<NodeEventArgs>? ChildRemoved;
    public event EventHandler<NameChangedEventArgs>? NameChanged;

    internal SceneNode(EntityId entityId, SceneNode? parent, string name)
    {
        EntityId = entityId;
        _parent = parent;
        _name = name;
        _children = new List<SceneNode>();
        _localTransform = Transform.Identity;
        _worldTransform = Transform.Identity;
    }

    /// <summary>
    /// Add a child node
    /// </summary>
    internal void AddChild(SceneNode child)
    {
        if (child == null)
            throw new ArgumentNullException(nameof(child));

        if (child == this)
            throw new InvalidOperationException("Cannot add node as child of itself");

        if (_children.Contains(child))
            return;

        // Check for circular references
        if (IsAncestorOf(child))
            throw new InvalidOperationException("Adding child would create circular reference");

        _children.Add(child);
        child._parent = this;

        ChildAdded?.Invoke(this, new NodeEventArgs(child));
    }

    /// <summary>
    /// Remove a child node
    /// </summary>
    internal void RemoveChild(SceneNode child)
    {
        if (child == null)
            return;

        if (_children.Remove(child))
        {
            child._parent = null;
            ChildRemoved?.Invoke(this, new NodeEventArgs(child));
        }
    }

    /// <summary>
    /// Set the parent of this node
    /// </summary>
    internal void SetParent(SceneNode? parent)
    {
        _parent = parent;
    }

    /// <summary>
    /// Find child by name
    /// </summary>
    public SceneNode? FindChild(string name)
    {
        return _children.FirstOrDefault(child => child.Name == name);
    }

    /// <summary>
    /// Find all children with the specified name
    /// </summary>
    public IEnumerable<SceneNode> FindChildren(string name)
    {
        return _children.Where(child => child.Name == name);
    }

    /// <summary>
    /// Find descendant by name (recursive search)
    /// </summary>
    public SceneNode? FindDescendant(string name)
    {
        // Check direct children first
        var child = FindChild(name);
        if (child != null)
            return child;

        // Recursively search children
        foreach (var childNode in _children)
        {
            var descendant = childNode.FindDescendant(name);
            if (descendant != null)
                return descendant;
        }

        return null;
    }

    /// <summary>
    /// Get all descendants (all children recursively)
    /// </summary>
    public IEnumerable<SceneNode> GetDescendants()
    {
        foreach (var child in _children)
        {
            yield return child;
            
            foreach (var descendant in child.GetDescendants())
            {
                yield return descendant;
            }
        }
    }

    /// <summary>
    /// Get all ancestors (parents up to root)
    /// </summary>
    public IEnumerable<SceneNode> GetAncestors()
    {
        var current = Parent;
        while (current != null && !current.IsRoot)
        {
            yield return current;
            current = current.Parent;
        }
    }

    /// <summary>
    /// Check if this node is an ancestor of another node
    /// </summary>
    public bool IsAncestorOf(SceneNode node)
    {
        if (node == null)
            return false;

        var current = node.Parent;
        while (current != null)
        {
            if (current == this)
                return true;
            current = current.Parent;
        }

        return false;
    }

    /// <summary>
    /// Check if this node is a descendant of another node
    /// </summary>
    public bool IsDescendantOf(SceneNode node)
    {
        return node != null && node.IsAncestorOf(this);
    }

    /// <summary>
    /// Get sibling nodes (other children of the same parent)
    /// </summary>
    public IEnumerable<SceneNode> GetSiblings()
    {
        if (Parent == null)
            return Enumerable.Empty<SceneNode>();

        return Parent.Children.Where(child => child != this);
    }

    /// <summary>
    /// Get the next sibling node
    /// </summary>
    public SceneNode? GetNextSibling()
    {
        if (Parent == null)
            return null;

        var siblings = Parent.Children;
        var index = siblings.ToList().IndexOf(this);
        
        return index >= 0 && index < siblings.Count - 1 ? siblings[index + 1] : null;
    }

    /// <summary>
    /// Get the previous sibling node
    /// </summary>
    public SceneNode? GetPreviousSibling()
    {
        if (Parent == null)
            return null;

        var siblings = Parent.Children;
        var index = siblings.ToList().IndexOf(this);
        
        return index > 0 ? siblings[index - 1] : null;
    }

    /// <summary>
    /// Move this node to a different position in the parent's children list
    /// </summary>
    public void MoveTo(int index)
    {
        if (Parent == null)
            return;

        var parentChildren = Parent._children;
        var currentIndex = parentChildren.IndexOf(this);
        
        if (currentIndex >= 0 && index != currentIndex)
        {
            parentChildren.RemoveAt(currentIndex);
            index = Math.Clamp(index, 0, parentChildren.Count);
            parentChildren.Insert(index, this);
        }
    }

    /// <summary>
    /// Move this node before the specified sibling
    /// </summary>
    public void MoveBefore(SceneNode sibling)
    {
        if (Parent == null || sibling == null || sibling.Parent != Parent)
            return;

        var parentChildren = Parent._children;
        var siblingIndex = parentChildren.IndexOf(sibling);
        
        if (siblingIndex >= 0)
        {
            MoveTo(siblingIndex);
        }
    }

    /// <summary>
    /// Move this node after the specified sibling
    /// </summary>
    public void MoveAfter(SceneNode sibling)
    {
        if (Parent == null || sibling == null || sibling.Parent != Parent)
            return;

        var parentChildren = Parent._children;
        var siblingIndex = parentChildren.IndexOf(sibling);
        
        if (siblingIndex >= 0)
        {
            MoveTo(siblingIndex + 1);
        }
    }

    private void OnTransformChanged()
    {
        TransformChanged?.Invoke(this, new TransformChangedEventArgs(_localTransform, _worldTransform));
    }

    private void OnNameChanged(string oldName)
    {
        NameChanged?.Invoke(this, new NameChangedEventArgs(oldName, _name));
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            // Clear all children (they should be disposed separately by scene graph)
            _children.Clear();
            _parent = null;
            _disposed = true;
        }
    }

    public override string ToString()
    {
        return $"{Name} (Entity: {EntityId}, Children: {ChildCount})";
    }
}

/// <summary>
/// Event arguments for transform changes
/// </summary>
public class TransformChangedEventArgs : EventArgs
{
    public Transform LocalTransform { get; }
    public Transform WorldTransform { get; }

    public TransformChangedEventArgs(Transform localTransform, Transform worldTransform)
    {
        LocalTransform = localTransform;
        WorldTransform = worldTransform;
    }
}

/// <summary>
/// Event arguments for node events
/// </summary>
public class NodeEventArgs : EventArgs
{
    public SceneNode Node { get; }

    public NodeEventArgs(SceneNode node)
    {
        Node = node;
    }
}

/// <summary>
/// Event arguments for name changes
/// </summary>
public class NameChangedEventArgs : EventArgs
{
    public string OldName { get; }
    public string NewName { get; }

    public NameChangedEventArgs(string oldName, string newName)
    {
        OldName = oldName;
        NewName = newName;
    }
}