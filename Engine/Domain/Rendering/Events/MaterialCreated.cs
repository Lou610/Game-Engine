using Engine.Domain.Rendering;

namespace Engine.Domain.Rendering.Events;

/// <summary>
/// Domain event for material creation
/// </summary>
public record MaterialCreated(string MaterialId, Material Material);

