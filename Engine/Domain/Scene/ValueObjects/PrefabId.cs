namespace Engine.Domain.Scene.ValueObjects;

/// <summary>
/// Prefab identifier
/// </summary>
public readonly record struct PrefabId
{
    public string Value { get; init; }

    public PrefabId(string value)
    {
        Value = value;
    }

    public static PrefabId Invalid => new(string.Empty);
}

