using System.Collections.Generic;
using Engine.Domain.Rendering;
using Engine.Domain.Rendering.Events;

namespace Engine.Domain.Rendering.Services;

/// <summary>
/// Material domain operations
/// </summary>
public class MaterialService
{
    private readonly Dictionary<string, Material> _materials = new();
    private readonly List<MaterialCreated> _events = new();

    public Material CreateMaterial(string id, Material material)
    {
        material.Id = id;
        _materials[id] = material;
        var evt = new MaterialCreated(id, material);
        _events.Add(evt);
        return material;
    }

    public Material? GetMaterial(string id)
    {
        return _materials.TryGetValue(id, out var material) ? material : null;
    }

    public IEnumerable<MaterialCreated> GetEvents() => _events;

    public void ClearEvents()
    {
        _events.Clear();
    }
}

