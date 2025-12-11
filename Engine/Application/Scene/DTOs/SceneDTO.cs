namespace Engine.Application.Scene.DTOs;

/// <summary>
/// Data transfer object for scene data
/// </summary>
public class SceneDTO
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int EntityCount { get; set; }
}

