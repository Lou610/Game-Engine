namespace Engine.Domain.Scene.ValueObjects;

/// <summary>
/// Scene identifier
/// </summary>
public readonly record struct SceneId
{
    public string Value { get; init; }

    public SceneId(string value)
    {
        Value = value;
    }

    public static SceneId Invalid => new(string.Empty);
}

