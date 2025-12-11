namespace Engine.Domain.Rendering.Events;

/// <summary>
/// Domain event for mesh loaded
/// </summary>
public record MeshLoaded(string MeshId, int VertexCount, int IndexCount);

