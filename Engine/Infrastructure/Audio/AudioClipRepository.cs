using System.Collections.Generic;
using Engine.Domain.Audio;
using Engine.Domain.Audio.ValueObjects;

namespace Engine.Infrastructure.Audio;

/// <summary>
/// Audio file loading and persistence
/// </summary>
public class AudioClipRepository
{
    private readonly Dictionary<string, AudioClip> _clips = new();

    public void Save(AudioClip clip)
    {
        _clips[clip.Id.Value] = clip;
    }

    public AudioClip? Load(AudioClipId id)
    {
        return _clips.TryGetValue(id.Value, out var clip) ? clip : null;
    }

    public void Delete(AudioClipId id)
    {
        _clips.Remove(id.Value);
    }
}

