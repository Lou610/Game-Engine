using Engine.Domain.ECS;
using Engine.Domain.Rendering.Entities;

namespace Engine.Domain.ECS.Components;

/// <summary>
/// Component for rendering meshes
/// </summary>
public class MeshRenderer : Component
{
    public Mesh? Mesh { get; set; }
    public Material? Material { get; set; }
    public bool IsEnabled { get; set; } = true;
    public bool CastShadows { get; set; } = true;
    public bool ReceiveShadows { get; set; } = true;
    public int RenderLayer { get; set; } = 0;
    
    public MeshRenderer() { }
    
    public MeshRenderer(Mesh mesh, Material material)
    {
        Mesh = mesh;
        Material = material;
    }
    
    /// <summary>
    /// Check if this renderer is ready to render
    /// </summary>
    public bool IsReadyToRender => IsEnabled && Mesh != null && Material != null;
}