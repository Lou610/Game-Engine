using System.Collections.Generic;
using Engine.Domain.Rendering;

namespace Engine.Infrastructure.Rendering;

/// <summary>
/// Material persistence
/// </summary>
public class MaterialRepository
{
    private readonly Dictionary<string, Material> _materials = new();

    public void Save(Material material)
    {
        _materials[material.Id] = material;
    }

    public Material? Load(string id)
    {
        return _materials.TryGetValue(id, out var material) ? material : null;
    }

    public void Delete(string id)
    {
        _materials.Remove(id);
    }
}

