using System;
using System.Collections.Generic;
using Engine.Domain.Resources.ValueObjects;

namespace Engine.Domain.Resources;

/// <summary>
/// Asset representing a 3D mesh with vertex and index data
/// </summary>
public class MeshAsset : Asset
{
    /// <summary>
    /// Vertex data for the mesh
    /// </summary>
    public float[] Vertices { get; set; } = Array.Empty<float>();

    /// <summary>
    /// Index data for triangle connectivity
    /// </summary>
    public uint[] Indices { get; set; } = Array.Empty<uint>();

    /// <summary>
    /// Number of vertices in the mesh
    /// </summary>
    public int VertexCount => Vertices.Length / VertexStride;

    /// <summary>
    /// Number of triangles in the mesh
    /// </summary>
    public int TriangleCount => Indices.Length / 3;

    /// <summary>
    /// Stride between vertices (number of floats per vertex)
    /// Default: 8 (position: 3, normal: 3, uv: 2)
    /// </summary>
    public int VertexStride { get; set; } = 8;

    /// <summary>
    /// Mesh bounding box minimum point
    /// </summary>
    public float[] BoundsMin { get; set; } = { 0f, 0f, 0f };

    /// <summary>
    /// Mesh bounding box maximum point
    /// </summary>
    public float[] BoundsMax { get; set; } = { 0f, 0f, 0f };

    /// <summary>
    /// Additional mesh attributes (e.g., vertex colors, additional UV sets)
    /// </summary>
    public Dictionary<string, float[]> Attributes { get; set; } = new();

    public MeshAsset()
    {
        Type = AssetType.Mesh;
    }

    public MeshAsset(AssetGuid id, string name, AssetPath path) : this()
    {
        Id = id;
        Name = name;
        Path = path;
    }

    /// <summary>
    /// Validate that the mesh data is consistent
    /// </summary>
    public bool IsValid()
    {
        return Vertices.Length > 0 && 
               Vertices.Length % VertexStride == 0 &&
               Indices.Length > 0 && 
               Indices.Length % 3 == 0;
    }

    /// <summary>
    /// Calculate the mesh bounds from vertex data
    /// </summary>
    public void CalculateBounds()
    {
        if (Vertices.Length < 3) return;

        var minX = float.MaxValue;
        var minY = float.MaxValue;
        var minZ = float.MaxValue;
        var maxX = float.MinValue;
        var maxY = float.MinValue;
        var maxZ = float.MinValue;

        // Assuming position is the first 3 floats of each vertex
        for (int i = 0; i < Vertices.Length; i += VertexStride)
        {
            var x = Vertices[i];
            var y = Vertices[i + 1];
            var z = Vertices[i + 2];

            if (x < minX) minX = x;
            if (x > maxX) maxX = x;
            if (y < minY) minY = y;
            if (y > maxY) maxY = y;
            if (z < minZ) minZ = z;
            if (z > maxZ) maxZ = z;
        }

        BoundsMin = new[] { minX, minY, minZ };
        BoundsMax = new[] { maxX, maxY, maxZ };
    }
}