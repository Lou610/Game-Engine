using Engine.Domain.Audio.ValueObjects;

namespace Engine.Domain.Audio;

/// <summary>
/// Entity representing audio emitter
/// </summary>
public class AudioSource
{
    public string Id { get; set; } = string.Empty;
    public AudioClipId ClipId { get; set; }
    public AudioSettings Settings { get; set; }
    public bool IsPlaying { get; set; }
}

