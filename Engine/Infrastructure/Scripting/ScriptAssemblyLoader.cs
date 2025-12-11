using System.Reflection;
using Engine.Domain.Scripting;

namespace Engine.Infrastructure.Scripting;

/// <summary>
/// Dynamic assembly loading
/// </summary>
public class ScriptAssemblyLoader
{
    public void LoadAssembly(ScriptAssembly assembly)
    {
        if (assembly.Assembly != null)
        {
            // Assembly is already loaded
            assembly.IsLoaded = true;
        }
    }

    public void UnloadAssembly(ScriptAssembly assembly)
    {
        assembly.IsLoaded = false;
        assembly.Assembly = null;
    }
}

