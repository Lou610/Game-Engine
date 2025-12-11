namespace Engine.Domain.Audio.ValueObjects;

/// <summary>
/// Volume, pitch, loop settings
/// </summary>
public readonly record struct AudioSettings
{
    public float Volume { get; init; }
    public float Pitch { get; init; }
    public bool Loop { get; init; }

    public AudioSettings(float volume = 1.0f, float pitch = 1.0f, bool loop = false)
    {
        Volume = volume;
        Pitch = pitch;
        Loop = loop;
    }
}

