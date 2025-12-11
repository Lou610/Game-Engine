using Engine.Domain.Scripting.ValueObjects;

namespace Engine.Domain.Scripting.Events;

/// <summary>
/// Domain event for script executed
/// </summary>
public record ScriptExecuted(ScriptId ScriptId);

