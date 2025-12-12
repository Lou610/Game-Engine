using System;
using System.Collections.Generic;
using System.Numerics;
using Engine.Domain.ECS.ValueObjects;
using Engine.Domain.Scene.ValueObjects;

namespace Engine.Domain.Scene.Serialization;

/// <summary>
/// Data transfer object for scene serialization
/// </summary>
public class SerializedScene
{
    public SceneMetadata Metadata { get; set; } = new();
    public SceneSettingsData Settings { get; set; } = new();
    public List<SerializedEntity> Entities { get; set; } = new();
    public List<SerializedSceneNode> SceneGraph { get; set; } = new();
    public Dictionary<string, object> CustomProperties { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime ModifiedAt { get; set; }
    public string Version { get; set; } = "1.0.0";
}

/// <summary>
/// Scene metadata for serialization
/// </summary>
public class SceneMetadata
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
}

/// <summary>
/// Scene settings for serialization
/// </summary>
public class SceneSettingsData
{
    public Vector4 BackgroundColor { get; set; }
    public Vector3 AmbientLight { get; set; }
    public FogSettingsData Fog { get; set; } = new();
    public PhysicsSettingsData Physics { get; set; } = new();
    public RenderingSettingsData Rendering { get; set; } = new();
    public AudioSettingsData Audio { get; set; } = new();
    public bool AutoSave { get; set; }
    public float AutoSaveInterval { get; set; }
    public int MaxEntities { get; set; }
    public float UpdateFrequency { get; set; }
}

/// <summary>
/// Fog settings for serialization
/// </summary>
public class FogSettingsData
{
    public bool Enabled { get; set; }
    public Vector3 Color { get; set; }
    public float Density { get; set; }
    public float Start { get; set; }
    public float End { get; set; }
    public string Mode { get; set; } = "Linear";
}

/// <summary>
/// Physics settings for serialization
/// </summary>
public class PhysicsSettingsData
{
    public bool Enabled { get; set; }
    public Vector3 Gravity { get; set; }
    public float TimeStep { get; set; }
    public int MaxSubSteps { get; set; }
    public int SolverIterations { get; set; }
    public bool ContinuousCollisionDetection { get; set; }
}

/// <summary>
/// Rendering settings for serialization
/// </summary>
public class RenderingSettingsData
{
    public bool ShadowsEnabled { get; set; }
    public int ShadowQuality { get; set; }
    public float ShadowDistance { get; set; }
    public bool PostProcessingEnabled { get; set; }
    public string AntiAliasing { get; set; } = "MSAA4x";
    public bool VSync { get; set; }
    public int TargetFrameRate { get; set; }
    public bool LodEnabled { get; set; }
    public bool FrustumCullingEnabled { get; set; }
    public bool OcclusionCullingEnabled { get; set; }
}

/// <summary>
/// Audio settings for serialization
/// </summary>
public class AudioSettingsData
{
    public float MasterVolume { get; set; }
    public bool SpatialAudioEnabled { get; set; }
    public bool DopplerEnabled { get; set; }
    public float SpeedOfSound { get; set; }
    public int MaxAudioSources { get; set; }
    public string RolloffMode { get; set; } = "Linear";
}

/// <summary>
/// Serialized entity data
/// </summary>
public class SerializedEntity
{
    public ulong Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<SerializedComponent> Components { get; set; } = new();
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Serialized component data
/// </summary>
public class SerializedComponent
{
    public string TypeName { get; set; } = string.Empty;
    public Dictionary<string, object> Data { get; set; } = new();
}

/// <summary>
/// Serialized scene node data
/// </summary>
public class SerializedSceneNode
{
    public ulong EntityId { get; set; }
    public string Name { get; set; } = string.Empty;
    public ulong? ParentEntityId { get; set; }
    public List<ulong> ChildEntityIds { get; set; } = new();
    public TransformData LocalTransform { get; set; } = new();
    public TransformData WorldTransform { get; set; } = new();
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Transform data for serialization
/// </summary>
public class TransformData
{
    public Vector3 Position { get; set; }
    public Vector4 Rotation { get; set; } // Quaternion as Vector4
    public Vector3 Scale { get; set; }

    public TransformData()
    {
        Position = Vector3.Zero;
        Rotation = new Vector4(0, 0, 0, 1); // Identity quaternion
        Scale = Vector3.One;
    }
}

/// <summary>
/// Asset reference data for serialization
/// </summary>
public class AssetReference
{
    public string AssetId { get; set; } = string.Empty;
    public string AssetType { get; set; } = string.Empty;
    public string AssetPath { get; set; } = string.Empty;
    public Dictionary<string, object> Metadata { get; set; } = new();
}