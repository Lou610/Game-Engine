using System;
using System.Runtime.InteropServices;
using SysVector3 = System.Numerics.Vector3;

namespace Engine.Domain.Rendering.ValueObjects;

/// <summary>
/// Vertex structure for mesh rendering.
/// Layout matches the Vulkan shader input (position + color for basic rendering).
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public readonly record struct Vertex(
    SysVector3 Position,
    SysVector3 Color)
{
    public static Vertex Create(SysVector3 position, SysVector3 color) =>
        new(position, color);
    
    public static Vertex Create(float x, float y, float z, float r, float g, float b) =>
        new(new SysVector3(x, y, z), new SysVector3(r, g, b));
}

/// <summary>
/// Bounding box for spatial calculations
/// </summary>
public readonly record struct BoundingBox(SysVector3 Min, SysVector3 Max)
{
    public SysVector3 Center => (Min + Max) * 0.5f;
    public SysVector3 Size => Max - Min;
    public SysVector3 Extents => Size * 0.5f;
    
    public static BoundingBox Empty => new(SysVector3.Zero, SysVector3.Zero);
    
    public static BoundingBox FromVertices(ReadOnlySpan<Vertex> vertices)
    {
        if (vertices.Length == 0)
            return Empty;
            
        var min = vertices[0].Position;
        var max = vertices[0].Position;
        
        for (int i = 1; i < vertices.Length; i++)
        {
            var pos = vertices[i].Position;
            min = SysVector3.Min(min, pos);
            max = SysVector3.Max(max, pos);
        }
        
        return new BoundingBox(min, max);
    }
    
    public bool Contains(SysVector3 point) =>
        point.X >= Min.X && point.X <= Max.X &&
        point.Y >= Min.Y && point.Y <= Max.Y &&
        point.Z >= Min.Z && point.Z <= Max.Z;
    
    public bool Intersects(BoundingBox other) =>
        Min.X <= other.Max.X && Max.X >= other.Min.X &&
        Min.Y <= other.Max.Y && Max.Y >= other.Min.Y &&
        Min.Z <= other.Max.Z && Max.Z >= other.Min.Z;
}

/// <summary>
/// Camera projection types
/// </summary>
public enum ProjectionType
{
    Perspective,
    Orthographic
}

/// <summary>
/// Rendering configuration value object
/// </summary>
public readonly record struct RenderSettings(
    bool EnableVSync = true,
    bool EnableValidation = true,
    uint MaxFramesInFlight = 2,
    string ApplicationName = "LAG Game Engine")
{
    public static RenderSettings Default => new();
}