using System.Collections.Generic;
using Engine.Domain.Scripting;
using Engine.Domain.Scripting.ValueObjects;

namespace Engine.Infrastructure.Scripting;

/// <summary>
/// Script file persistence
/// </summary>
public class ScriptRepository
{
    private readonly Dictionary<string, Script> _scripts = new();

    public void Save(Script script)
    {
        _scripts[script.Id.Value] = script;
    }

    public Script? Load(ScriptId id)
    {
        return _scripts.TryGetValue(id.Value, out var script) ? script : null;
    }

    public void Delete(ScriptId id)
    {
        _scripts.Remove(id.Value);
    }

    public IEnumerable<Script> GetAll()
    {
        return _scripts.Values;
    }
}

