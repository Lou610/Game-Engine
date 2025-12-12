using System;
using System.Collections.Generic;
using System.Linq;
using Engine.Domain.Rendering.ValueObjects;
using SysVector3 = System.Numerics.Vector3;
using SysVector2 = System.Numerics.Vector2;

namespace Engine.Domain.Rendering.Entities;

/// <summary>
/// Domain entity representing a 3D mesh
/// </summary>
public class Mesh
{
    private readonly List<Vertex> _vertices;
    private readonly List<uint> _indices;
    private BoundingBox _bounds;
    private bool _boundsDirty = true;
    
    public MeshId Id { get; private set; }
    public string Name { get; private set; }
    public IReadOnlyList<Vertex> Vertices => _vertices.AsReadOnly();
    public IReadOnlyList<uint> Indices => _indices.AsReadOnly();
    
    public BoundingBox Bounds
    {
        get
        {
            if (_boundsDirty)
            {
                CalculateBounds();
            }
            return _bounds;
        }
    }
    
    public int VertexCount => _vertices.Count;
    public int IndexCount => _indices.Count;
    public int TriangleCount => _indices.Count / 3;
    
    public Mesh(string name, IEnumerable<Vertex> vertices, IEnumerable<uint> indices)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Mesh name cannot be null or empty", nameof(name));
            
        Id = MeshId.NewId();
        Name = name;
        _vertices = new List<Vertex>(vertices ?? throw new ArgumentNullException(nameof(vertices)));
        _indices = new List<uint>(indices ?? throw new ArgumentNullException(nameof(indices)));
        _boundsDirty = true;
        
        ValidateMesh();
    }
    
    public Mesh(string name, Vertex[] vertices, uint[] indices)
        : this(name, vertices.AsEnumerable(), indices.AsEnumerable())
    {
    }
    
    /// <summary>
    /// Update the mesh vertices
    /// </summary>
    public void UpdateVertices(IEnumerable<Vertex> vertices)
    {
        _vertices.Clear();
        _vertices.AddRange(vertices ?? throw new ArgumentNullException(nameof(vertices)));
        _boundsDirty = true;
        ValidateMesh();
    }
    
    /// <summary>
    /// Update the mesh indices
    /// </summary>
    public void UpdateIndices(IEnumerable<uint> indices)
    {
        _indices.Clear();
        _indices.AddRange(indices ?? throw new ArgumentNullException(nameof(indices)));
        ValidateMesh();
    }
    
    /// <summary>
    /// Add vertices to the mesh
    /// </summary>
    public void AddVertices(IEnumerable<Vertex> vertices)
    {
        _vertices.AddRange(vertices ?? throw new ArgumentNullException(nameof(vertices)));
        _boundsDirty = true;
    }
    
    /// <summary>
    /// Add indices to the mesh
    /// </summary>
    public void AddIndices(IEnumerable<uint> indices)
    {
        _indices.AddRange(indices ?? throw new ArgumentNullException(nameof(indices)));
        ValidateMesh();
    }
    
    /// <summary>
    /// Clear all mesh data
    /// </summary>
    public void Clear()
    {
        _vertices.Clear();
        _indices.Clear();
        _bounds = BoundingBox.Empty;
        _boundsDirty = true;
    }
    
    /// <summary>
    /// Create a copy of this mesh with a new name
    /// </summary>
    public Mesh Clone(string newName)
    {
        return new Mesh(newName, _vertices, _indices);
    }
    
    private void CalculateBounds()
    {
        if (_vertices.Count == 0)
        {
            _bounds = BoundingBox.Empty;
        }
        else
        {
            _bounds = BoundingBox.FromVertices(_vertices.ToArray().AsSpan());
        }
        _boundsDirty = false;
    }
    
    private void ValidateMesh()
    {
        // Check that all indices are valid
        var maxIndex = (uint)_vertices.Count;
        foreach (var index in _indices)
        {
            if (index >= maxIndex)
            {
                throw new InvalidOperationException($"Index {index} is out of range for {_vertices.Count} vertices");
            }
        }
        
        // Check that we have triangles (indices should be multiple of 3)
        if (_indices.Count % 3 != 0)
        {
            throw new InvalidOperationException($"Index count {_indices.Count} is not a multiple of 3");
        }
    }
}

/// <summary>
/// Static factory for creating common mesh shapes
/// </summary>
public static class MeshFactory
{
    /// <summary>
    /// Create a unit cube mesh
    /// </summary>
    public static Mesh CreateCube(string name = "Cube")
    {
        var vertices = new[]
        {
            // Front face (red)
            new Vertex(new SysVector3(-0.5f, -0.5f,  0.5f), new SysVector3(1, 0, 0)),
            new Vertex(new SysVector3( 0.5f, -0.5f,  0.5f), new SysVector3(1, 0, 0)),
            new Vertex(new SysVector3( 0.5f,  0.5f,  0.5f), new SysVector3(1, 0, 0)),
            new Vertex(new SysVector3(-0.5f,  0.5f,  0.5f), new SysVector3(1, 0, 0)),
            
            // Back face (green)
            new Vertex(new SysVector3(-0.5f, -0.5f, -0.5f), new SysVector3(0, 1, 0)),
            new Vertex(new SysVector3(-0.5f,  0.5f, -0.5f), new SysVector3(0, 1, 0)),
            new Vertex(new SysVector3( 0.5f,  0.5f, -0.5f), new SysVector3(0, 1, 0)),
            new Vertex(new SysVector3( 0.5f, -0.5f, -0.5f), new SysVector3(0, 1, 0)),
            
            // Left face (blue)
            new Vertex(new SysVector3(-0.5f, -0.5f, -0.5f), new SysVector3(0, 0, 1)),
            new Vertex(new SysVector3(-0.5f, -0.5f,  0.5f), new SysVector3(0, 0, 1)),
            new Vertex(new SysVector3(-0.5f,  0.5f,  0.5f), new SysVector3(0, 0, 1)),
            new Vertex(new SysVector3(-0.5f,  0.5f, -0.5f), new SysVector3(0, 0, 1)),
            
            // Right face (yellow)
            new Vertex(new SysVector3( 0.5f, -0.5f,  0.5f), new SysVector3(1, 1, 0)),
            new Vertex(new SysVector3( 0.5f, -0.5f, -0.5f), new SysVector3(1, 1, 0)),
            new Vertex(new SysVector3( 0.5f,  0.5f, -0.5f), new SysVector3(1, 1, 0)),
            new Vertex(new SysVector3( 0.5f,  0.5f,  0.5f), new SysVector3(1, 1, 0)),
            
            // Top face (magenta)
            new Vertex(new SysVector3(-0.5f,  0.5f,  0.5f), new SysVector3(1, 0, 1)),
            new Vertex(new SysVector3( 0.5f,  0.5f,  0.5f), new SysVector3(1, 0, 1)),
            new Vertex(new SysVector3( 0.5f,  0.5f, -0.5f), new SysVector3(1, 0, 1)),
            new Vertex(new SysVector3(-0.5f,  0.5f, -0.5f), new SysVector3(1, 0, 1)),
            
            // Bottom face (cyan)
            new Vertex(new SysVector3(-0.5f, -0.5f, -0.5f), new SysVector3(0, 1, 1)),
            new Vertex(new SysVector3( 0.5f, -0.5f, -0.5f), new SysVector3(0, 1, 1)),
            new Vertex(new SysVector3( 0.5f, -0.5f,  0.5f), new SysVector3(0, 1, 1)),
            new Vertex(new SysVector3(-0.5f, -0.5f,  0.5f), new SysVector3(0, 1, 1))
        };
        
        var indices = new uint[]
        {
            // Front face
            0, 1, 2,  0, 2, 3,
            // Back face
            4, 5, 6,  4, 6, 7,
            // Left face
            8, 9, 10,  8, 10, 11,
            // Right face
            12, 13, 14,  12, 14, 15,
            // Top face
            16, 17, 18,  16, 18, 19,
            // Bottom face
            20, 21, 22,  20, 22, 23
        };
        
        return new Mesh(name, vertices, indices);
    }
    
    /// <summary>
    /// Create a unit quad mesh
    /// </summary>
    public static Mesh CreateQuad(string name = "Quad")
    {
        var vertices = new[]
        {
            new Vertex(new SysVector3(-0.5f, -0.5f, 0), new SysVector3(1, 1, 1)),  // White
            new Vertex(new SysVector3( 0.5f, -0.5f, 0), new SysVector3(1, 0, 0)),  // Red
            new Vertex(new SysVector3( 0.5f,  0.5f, 0), new SysVector3(0, 1, 0)),  // Green
            new Vertex(new SysVector3(-0.5f,  0.5f, 0), new SysVector3(0, 0, 1))   // Blue
        };
        
        var indices = new uint[] { 0, 1, 2, 0, 2, 3 };
        
        return new Mesh(name, vertices, indices);
    }
}