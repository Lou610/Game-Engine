using System;
using System.Collections.Generic;
using Engine.Domain.Rendering.ValueObjects;
using Matrix4x4 = System.Numerics.Matrix4x4;
using Vector4 = System.Numerics.Vector4;

namespace Engine.Domain.Rendering.Entities;

/// <summary>
/// Domain entity representing a rendering material
/// </summary>
public class Material
{
    private readonly Dictionary<string, object> _properties;
    
    public MaterialId Id { get; private set; }
    public string Name { get; private set; }
    public ShaderId ShaderId { get; private set; }
    public IReadOnlyDictionary<string, object> Properties => _properties.AsReadOnly();
    
    public Material(string name, ShaderId shaderId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Material name cannot be null or empty", nameof(name));
            
        Id = MaterialId.NewId();
        Name = name;
        ShaderId = shaderId;
        _properties = new Dictionary<string, object>();
    }
    
    /// <summary>
    /// Set a material property value
    /// </summary>
    public void SetProperty<T>(string name, T value)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Property name cannot be null or empty", nameof(name));
            
        _properties[name] = value ?? throw new ArgumentNullException(nameof(value));
    }
    
    /// <summary>
    /// Get a material property value
    /// </summary>
    public T? GetProperty<T>(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Property name cannot be null or empty", nameof(name));
            
        if (_properties.TryGetValue(name, out var value) && value is T typedValue)
        {
            return typedValue;
        }
        
        return default;
    }
    
    /// <summary>
    /// Check if material has a property
    /// </summary>
    public bool HasProperty(string name) => 
        !string.IsNullOrWhiteSpace(name) && _properties.ContainsKey(name);
    
    /// <summary>
    /// Remove a property from the material
    /// </summary>
    public bool RemoveProperty(string name) =>
        !string.IsNullOrWhiteSpace(name) && _properties.Remove(name);
    
    /// <summary>
    /// Set texture property
    /// </summary>
    public void SetTexture(string name, TextureId textureId)
    {
        SetProperty(name, textureId);
    }
    
    /// <summary>
    /// Get texture property
    /// </summary>
    public TextureId? GetTexture(string name)
    {
        return GetProperty<TextureId>(name);
    }
    
    /// <summary>
    /// Set color property
    /// </summary>
    public void SetColor(string name, Vector4 color)
    {
        SetProperty(name, color);
    }
    
    /// <summary>
    /// Get color property
    /// </summary>
    public Vector4? GetColor(string name)
    {
        return GetProperty<Vector4>(name);
    }
    
    /// <summary>
    /// Set float property
    /// </summary>
    public void SetFloat(string name, float value)
    {
        SetProperty(name, value);
    }
    
    /// <summary>
    /// Get float property
    /// </summary>
    public float? GetFloat(string name)
    {
        return GetProperty<float>(name);
    }
    
    /// <summary>
    /// Set matrix property
    /// </summary>
    public void SetMatrix(string name, Matrix4x4 matrix)
    {
        SetProperty(name, matrix);
    }
    
    /// <summary>
    /// Get matrix property
    /// </summary>
    public Matrix4x4? GetMatrix(string name)
    {
        return GetProperty<Matrix4x4>(name);
    }
    
    /// <summary>
    /// Create a copy of this material with a new name
    /// </summary>
    public Material Clone(string newName)
    {
        var clone = new Material(newName, ShaderId);
        foreach (var (key, value) in _properties)
        {
            clone._properties[key] = value;
        }
        return clone;
    }
}

/// <summary>
/// Static factory for creating common materials
/// </summary>
public static class MaterialFactory
{
    /// <summary>
    /// Create a default unlit material
    /// </summary>
    public static Material CreateDefault(string name = "Default")
    {
        var material = new Material(name, ShaderId.Empty); // Will be set by renderer
        material.SetColor("_Color", Vector4.One);
        material.SetFloat("_Metallic", 0.0f);
        material.SetFloat("_Smoothness", 0.5f);
        return material;
    }
    
    /// <summary>
    /// Create a colored material
    /// </summary>
    public static Material CreateColored(string name, Vector4 color)
    {
        var material = CreateDefault(name);
        material.SetColor("_Color", color);
        return material;
    }
}