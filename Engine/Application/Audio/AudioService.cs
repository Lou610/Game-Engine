using Engine.Domain.Audio;
using Engine.Domain.Audio.Services;

namespace Engine.Application.Audio;

/// <summary>
/// Application service for audio orchestration
/// </summary>
public class AudioService
{
    private readonly AudioMixing _mixing;
    private readonly SpatialAudio _spatialAudio;

    public AudioService(AudioMixing mixing, SpatialAudio spatialAudio)
    {
        _mixing = mixing;
        _spatialAudio = spatialAudio;
    }

    public void PlayAudio(AudioSource source)
    {
        // Audio playback orchestration
    }
}

