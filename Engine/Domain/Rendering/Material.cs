using Engine.Domain.Rendering.ValueObjects;

namespace Engine.Domain.Rendering;

/// <summary>
/// Aggregate root with shader and texture references
/// </summary>
public class Material
{
    public string Id { get; set; } = string.Empty;
    public ShaderId ShaderId { get; set; }
    public TextureId DiffuseTextureId { get; set; }
    public Color Color { get; set; } = Color.White;
}

