using Engine.Domain.Scene.ValueObjects;

namespace Engine.Domain.Scene.Events;

/// <summary>
/// Domain event for scene unloaded
/// </summary>
public record SceneUnloaded(SceneId SceneId);

