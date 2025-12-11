using Engine.Domain.Scripting.ValueObjects;

namespace Engine.Domain.Scripting.Events;

/// <summary>
/// Domain event for script compiled
/// </summary>
public record ScriptCompiled(ScriptId ScriptId, bool Success);

