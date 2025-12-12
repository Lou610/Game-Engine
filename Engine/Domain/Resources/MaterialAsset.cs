using System;
using System.Collections.Generic;
using Engine.Domain.Resources.ValueObjects;

namespace Engine.Domain.Resources;

/// <summary>
/// Asset representing a material with shader parameters and textures
/// </summary>
public class MaterialAsset : Asset
{
    /// <summary>
    /// Name or path of the shader used by this material
    /// </summary>
    public string ShaderName { get; set; } = string.Empty;

    /// <summary>
    /// Shader uniform parameters (floats, vectors, matrices)
    /// </summary>
    public Dictionary<string, object> Parameters { get; set; } = new();

    /// <summary>
    /// Texture references used by this material
    /// </summary>
    public Dictionary<string, AssetGuid> Textures { get; set; } = new();

    /// <summary>
    /// Rendering properties (blend mode, culling, etc.)
    /// </summary>
    public MaterialProperties Properties { get; set; } = new();

    public MaterialAsset()
    {
        Type = AssetType.Material;
    }

    public MaterialAsset(AssetGuid id, string name, AssetPath path, string shaderName) : this()
    {
        Id = id;
        Name = name;
        Path = path;
        ShaderName = shaderName;
    }

    /// <summary>
    /// Set a float parameter
    /// </summary>
    public void SetFloat(string name, float value)
    {
        Parameters[name] = value;
    }

    /// <summary>
    /// Set a vector parameter (represented as float array)
    /// </summary>
    public void SetVector(string name, float[] value)
    {
        if (value.Length < 2 || value.Length > 4)
            throw new ArgumentException("Vector must have 2, 3, or 4 components", nameof(value));
        
        Parameters[name] = value;
    }

    /// <summary>
    /// Set a color parameter (RGBA as float array)
    /// </summary>
    public void SetColor(string name, float r, float g, float b, float a = 1.0f)
    {
        Parameters[name] = new[] { r, g, b, a };
    }

    /// <summary>
    /// Set a texture reference
    /// </summary>
    public void SetTexture(string name, AssetGuid textureAssetId)
    {
        Textures[name] = textureAssetId;
    }

    /// <summary>
    /// Get a parameter value, returning default if not found
    /// </summary>
    public T? GetParameter<T>(string name, T? defaultValue = default)
    {
        if (Parameters.TryGetValue(name, out var value) && value is T typedValue)
        {
            return typedValue;
        }
        return defaultValue;
    }

    /// <summary>
    /// Get a texture reference
    /// </summary>
    public AssetGuid? GetTexture(string name)
    {
        return Textures.TryGetValue(name, out var textureId) ? textureId : null;
    }

    /// <summary>
    /// Check if the material has all required parameters
    /// </summary>
    public bool IsValid()
    {
        return !string.IsNullOrEmpty(ShaderName);
    }
}

/// <summary>
/// Material rendering properties
/// </summary>
public class MaterialProperties
{
    /// <summary>
    /// Blend mode for transparency
    /// </summary>
    public BlendMode BlendMode { get; set; } = BlendMode.Opaque;

    /// <summary>
    /// Face culling mode
    /// </summary>
    public CullMode CullMode { get; set; } = CullMode.Back;

    /// <summary>
    /// Whether to write to depth buffer
    /// </summary>
    public bool DepthWrite { get; set; } = true;

    /// <summary>
    /// Depth test function
    /// </summary>
    public DepthTest DepthTest { get; set; } = DepthTest.Less;

    /// <summary>
    /// Whether the material is transparent
    /// </summary>
    public bool IsTransparent => BlendMode != BlendMode.Opaque;
}

public enum BlendMode
{
    Opaque,
    Alpha,
    Additive,
    Multiply
}

public enum CullMode
{
    Off,
    Front,
    Back
}

public enum DepthTest
{
    Off,
    Never,
    Less,
    Equal,
    LessOrEqual,
    Greater,
    NotEqual,
    GreaterOrEqual,
    Always
}