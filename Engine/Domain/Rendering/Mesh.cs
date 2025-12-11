namespace Engine.Domain.Rendering;

/// <summary>
/// Aggregate root with vertex and index data
/// </summary>
public class Mesh
{
    public string Id { get; set; } = string.Empty;
    public float[] Vertices { get; set; } = Array.Empty<float>();
    public uint[] Indices { get; set; } = Array.Empty<uint>();
    public int VertexCount { get; set; }
    public int IndexCount { get; set; }
}

