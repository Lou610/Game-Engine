using System.Reflection;

namespace Engine.Domain.Scripting;

/// <summary>
/// Entity representing compiled assembly
/// </summary>
public class ScriptAssembly
{
    public string Id { get; set; } = string.Empty;
    public Assembly? Assembly { get; set; }
    public bool IsLoaded { get; set; }
}

