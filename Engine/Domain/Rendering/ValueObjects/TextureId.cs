namespace Engine.Domain.Rendering.ValueObjects;

/// <summary>
/// Texture identifier
/// </summary>
public readonly record struct TextureId
{
    public string Value { get; init; }

    public TextureId(string value)
    {
        Value = value;
    }

    public static TextureId Invalid => new(string.Empty);
}

