namespace Engine.Domain.Audio.ValueObjects;

/// <summary>
/// Audio clip identifier
/// </summary>
public readonly record struct AudioClipId
{
    public string Value { get; init; }

    public AudioClipId(string value)
    {
        Value = value;
    }

    public static AudioClipId Invalid => new(string.Empty);
}

