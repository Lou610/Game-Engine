using Engine.Domain.Scripting.ValueObjects;

namespace Engine.Domain.Scripting;

/// <summary>
/// Aggregate root representing a script
/// </summary>
public class Script
{
    public ScriptId Id { get; set; }
    public ScriptSource Source { get; set; }
    public string FilePath { get; set; } = string.Empty;
}

