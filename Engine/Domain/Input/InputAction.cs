using Engine.Domain.Input.ValueObjects;

namespace Engine.Domain.Input;

/// <summary>
/// Entity representing user action
/// </summary>
public class InputAction
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public InputState State { get; set; }
}

