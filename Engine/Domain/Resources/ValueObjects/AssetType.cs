namespace Engine.Domain.Resources.ValueObjects;

/// <summary>
/// Asset type (texture, mesh, audio, etc.)
/// </summary>
public enum AssetType
{
    Unknown = 0,
    Texture = 1,
    Mesh = 2,
    Audio = 3,
    Script = 4,
    Shader = 5,
    Material = 6,
    Scene = 7,
    Prefab = 8
}

