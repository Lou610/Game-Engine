using Engine.Domain.Audio;
using Engine.Domain.Rendering.ValueObjects;

namespace Engine.Domain.Audio.Services;

/// <summary>
/// Domain service for 3D audio positioning
/// </summary>
public class SpatialAudio
{
    public float CalculateVolume(AudioSource source, Vector3 listenerPosition, Vector3 sourcePosition)
    {
        // 3D audio volume calculation based on distance
        return 1.0f;
    }
}

