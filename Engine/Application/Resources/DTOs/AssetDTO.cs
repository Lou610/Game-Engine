namespace Engine.Application.Resources.DTOs;

/// <summary>
/// Data transfer object for asset data
/// </summary>
public class AssetDTO
{
    public string Id { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

