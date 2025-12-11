using Engine.Domain.Scene.ValueObjects;

namespace Engine.Domain.Scene.Events;

/// <summary>
/// Domain event for scene loaded
/// </summary>
public record SceneLoaded(SceneId SceneId, string SceneName);

