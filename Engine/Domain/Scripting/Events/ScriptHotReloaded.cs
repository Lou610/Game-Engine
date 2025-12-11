using Engine.Domain.Scripting.ValueObjects;

namespace Engine.Domain.Scripting.Events;

/// <summary>
/// Domain event for script hot reloaded
/// </summary>
public record ScriptHotReloaded(ScriptId ScriptId);

