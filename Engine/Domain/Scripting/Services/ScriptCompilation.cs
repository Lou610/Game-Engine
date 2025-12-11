using Engine.Domain.Scripting;
using Engine.Domain.Scripting.ValueObjects;

namespace Engine.Domain.Scripting.Services;

/// <summary>
/// Domain service for compilation logic
/// </summary>
public class ScriptCompilation
{
    public ScriptAssembly Compile(Script script)
    {
        // Compilation logic would be handled by infrastructure
        return new ScriptAssembly
        {
            Id = script.Id.Value,
            IsLoaded = false
        };
    }
}

