using Engine.Domain.ECS;

namespace Engine.Domain.ECS.Components;

/// <summary>
/// Entity with mesh and material references
/// </summary>
public class Renderable : Component
{
    public string MeshId { get; set; } = string.Empty;
    public string MaterialId { get; set; } = string.Empty;
}

