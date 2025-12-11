namespace Engine.Domain.Rendering.Events;

/// <summary>
/// Domain event for render command queued
/// </summary>
public record RenderCommandQueued(string MeshId, string MaterialId, int InstanceCount);

