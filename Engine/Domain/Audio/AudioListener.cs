namespace Engine.Domain.Audio;

/// <summary>
/// Entity representing audio receiver
/// </summary>
public class AudioListener
{
    public string Id { get; set; } = string.Empty;
    public float Volume { get; set; } = 1.0f;
}

