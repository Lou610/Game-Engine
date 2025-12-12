using System.Collections.Generic;
using Engine.Domain.Rendering.ValueObjects;

namespace Engine.Infrastructure.Rendering;

/// <summary>
/// Shader persistence
/// </summary>
public class ShaderRepository
{
    private readonly Dictionary<string, string> _shaders = new();

    public void Save(ShaderId id, string shaderSource)
    {
        _shaders[id.Value.ToString()] = shaderSource;
    }

    public string? Load(ShaderId id)
    {
        return _shaders.TryGetValue(id.Value.ToString(), out var source) ? source : null;
    }

    public void Delete(ShaderId id)
    {
        _shaders.Remove(id.Value.ToString());
    }
}

