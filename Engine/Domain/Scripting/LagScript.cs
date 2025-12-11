using Engine.Domain.Scripting.ValueObjects;

namespace Engine.Domain.Scripting;

/// <summary>
/// Entity representing lag script source
/// </summary>
public class LagScript
{
    public ScriptId Id { get; set; }
    public string SourceCode { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
}

