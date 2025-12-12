using System;
using System.Collections.Generic;
using System.Numerics;

namespace Engine.Domain.Scene;

/// <summary>
/// Configuration settings for a scene
/// </summary>
public class SceneSettings
{
    /// <summary>
    /// Scene background color
    /// </summary>
    public Vector4 BackgroundColor { get; set; } = new Vector4(0.1f, 0.1f, 0.1f, 1.0f);

    /// <summary>
    /// Ambient lighting color
    /// </summary>
    public Vector3 AmbientLight { get; set; } = new Vector3(0.2f, 0.2f, 0.2f);

    /// <summary>
    /// Fog settings
    /// </summary>
    public FogSettings Fog { get; set; } = new FogSettings();

    /// <summary>
    /// Physics settings for the scene
    /// </summary>
    public PhysicsSettings Physics { get; set; } = new PhysicsSettings();

    /// <summary>
    /// Rendering settings
    /// </summary>
    public RenderingSettings Rendering { get; set; } = new RenderingSettings();

    /// <summary>
    /// Audio settings
    /// </summary>
    public AudioSettings Audio { get; set; } = new AudioSettings();

    /// <summary>
    /// Custom properties for scene-specific configurations
    /// </summary>
    public Dictionary<string, object> CustomProperties { get; set; } = new Dictionary<string, object>();

    /// <summary>
    /// Whether the scene should auto-save changes
    /// </summary>
    public bool AutoSave { get; set; } = true;

    /// <summary>
    /// Auto-save interval in seconds
    /// </summary>
    public float AutoSaveInterval { get; set; } = 300.0f; // 5 minutes

    /// <summary>
    /// Maximum number of entities allowed in the scene (0 = unlimited)
    /// </summary>
    public int MaxEntities { get; set; } = 0;

    /// <summary>
    /// Scene update frequency (updates per second, 0 = use engine default)
    /// </summary>
    public float UpdateFrequency { get; set; } = 0.0f;
}

/// <summary>
/// Fog rendering settings
/// </summary>
public class FogSettings
{
    /// <summary>
    /// Whether fog is enabled
    /// </summary>
    public bool Enabled { get; set; } = false;

    /// <summary>
    /// Fog color
    /// </summary>
    public Vector3 Color { get; set; } = Vector3.One;

    /// <summary>
    /// Fog density for exponential fog
    /// </summary>
    public float Density { get; set; } = 0.01f;

    /// <summary>
    /// Fog start distance for linear fog
    /// </summary>
    public float Start { get; set; } = 10.0f;

    /// <summary>
    /// Fog end distance for linear fog
    /// </summary>
    public float End { get; set; } = 100.0f;

    /// <summary>
    /// Fog mode (Linear, Exponential, ExponentialSquared)
    /// </summary>
    public FogMode Mode { get; set; } = FogMode.Linear;
}

/// <summary>
/// Physics simulation settings
/// </summary>
public class PhysicsSettings
{
    /// <summary>
    /// Whether physics simulation is enabled
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gravity vector
    /// </summary>
    public Vector3 Gravity { get; set; } = new Vector3(0, -9.81f, 0);

    /// <summary>
    /// Physics time step
    /// </summary>
    public float TimeStep { get; set; } = 1.0f / 60.0f;

    /// <summary>
    /// Maximum physics sub-steps per frame
    /// </summary>
    public int MaxSubSteps { get; set; } = 3;

    /// <summary>
    /// Physics solver iterations
    /// </summary>
    public int SolverIterations { get; set; } = 10;

    /// <summary>
    /// Whether to use continuous collision detection
    /// </summary>
    public bool ContinuousCollisionDetection { get; set; } = true;
}

/// <summary>
/// Rendering settings for the scene
/// </summary>
public class RenderingSettings
{
    /// <summary>
    /// Whether shadows are enabled
    /// </summary>
    public bool ShadowsEnabled { get; set; } = true;

    /// <summary>
    /// Shadow quality level (0-3)
    /// </summary>
    public int ShadowQuality { get; set; } = 2;

    /// <summary>
    /// Maximum shadow distance
    /// </summary>
    public float ShadowDistance { get; set; } = 100.0f;

    /// <summary>
    /// Whether post-processing is enabled
    /// </summary>
    public bool PostProcessingEnabled { get; set; } = true;

    /// <summary>
    /// Anti-aliasing mode
    /// </summary>
    public AntiAliasingMode AntiAliasing { get; set; } = AntiAliasingMode.MSAA4x;

    /// <summary>
    /// V-Sync setting
    /// </summary>
    public bool VSync { get; set; } = true;

    /// <summary>
    /// Target frame rate (0 = unlimited)
    /// </summary>
    public int TargetFrameRate { get; set; } = 60;

    /// <summary>
    /// Level of detail (LOD) enabled
    /// </summary>
    public bool LodEnabled { get; set; } = true;

    /// <summary>
    /// Frustum culling enabled
    /// </summary>
    public bool FrustumCullingEnabled { get; set; } = true;

    /// <summary>
    /// Occlusion culling enabled
    /// </summary>
    public bool OcclusionCullingEnabled { get; set; } = false;
}

/// <summary>
/// Audio settings for the scene
/// </summary>
public class AudioSettings
{
    /// <summary>
    /// Master volume for the scene (0.0 to 1.0)
    /// </summary>
    public float MasterVolume { get; set; } = 1.0f;

    /// <summary>
    /// 3D audio enabled
    /// </summary>
    public bool SpatialAudioEnabled { get; set; } = true;

    /// <summary>
    /// Doppler effect enabled
    /// </summary>
    public bool DopplerEnabled { get; set; } = true;

    /// <summary>
    /// Speed of sound for doppler calculations
    /// </summary>
    public float SpeedOfSound { get; set; } = 343.0f; // meters per second

    /// <summary>
    /// Maximum audio sources that can play simultaneously
    /// </summary>
    public int MaxAudioSources { get; set; } = 32;

    /// <summary>
    /// Audio rolloff model
    /// </summary>
    public AudioRolloffMode RolloffMode { get; set; } = AudioRolloffMode.Linear;
}

/// <summary>
/// Fog rendering modes
/// </summary>
public enum FogMode
{
    Linear,
    Exponential,
    ExponentialSquared
}

/// <summary>
/// Anti-aliasing modes
/// </summary>
public enum AntiAliasingMode
{
    None,
    MSAA2x,
    MSAA4x,
    MSAA8x,
    FXAA,
    TAA
}

/// <summary>
/// Audio rolloff modes for 3D audio
/// </summary>
public enum AudioRolloffMode
{
    Linear,
    Logarithmic,
    Custom
}