namespace Engine.Domain.Rendering.ValueObjects;

/// <summary>
/// Shader identifier
/// </summary>
public readonly record struct ShaderId
{
    public string Value { get; init; }

    public ShaderId(string value)
    {
        Value = value;
    }

    public static ShaderId Invalid => new(string.Empty);
}

