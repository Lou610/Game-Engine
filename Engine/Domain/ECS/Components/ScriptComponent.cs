using Engine.Domain.ECS;

namespace Engine.Domain.ECS.Components;

/// <summary>
/// Entity with script reference
/// </summary>
public class ScriptComponent : Component
{
    public string ScriptId { get; set; } = string.Empty;
    public string ScriptType { get; set; } = string.Empty;
}

