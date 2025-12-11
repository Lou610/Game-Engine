namespace Engine.Domain.Resources.ValueObjects;

/// <summary>
/// Asset file path
/// </summary>
public readonly record struct AssetPath
{
    public string Value { get; init; }

    public AssetPath(string value)
    {
        Value = value;
    }

    public static AssetPath Invalid => new(string.Empty);
}

