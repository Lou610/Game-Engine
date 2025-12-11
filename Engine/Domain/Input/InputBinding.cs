using Engine.Domain.Input.ValueObjects;

namespace Engine.Domain.Input;

/// <summary>
/// Entity mapping actions to input devices
/// </summary>
public class InputBinding
{
    public string Id { get; set; } = string.Empty;
    public string ActionId { get; set; } = string.Empty;
    public KeyCode Key { get; set; }
}

