namespace Engine.Domain.Rendering.ValueObjects;

/// <summary>
/// Texture identifier
/// </summary>
public readonly record struct TextureId(System.Guid Value)
{
    public static TextureId NewId() => new(System.Guid.NewGuid());
    public static TextureId Empty => new(System.Guid.Empty);
    
    public override string ToString() => Value.ToString();
}

