using Engine.Domain.Audio.ValueObjects;

namespace Engine.Domain.Audio;

/// <summary>
/// Aggregate root with audio data
/// </summary>
public class AudioClip
{
    public AudioClipId Id { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public int SampleRate { get; set; }
    public int Channels { get; set; }
    public float Duration { get; set; }
}

