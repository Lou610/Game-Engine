namespace Engine.Application.Rendering;

/// <summary>
/// DTO for render commands
/// </summary>
public class RenderCommand
{
    public string MeshId { get; set; } = string.Empty;
    public string MaterialId { get; set; } = string.Empty;
    public int InstanceCount { get; set; } = 1;
}

