using Engine.Domain.Input.ValueObjects;

namespace Engine.Infrastructure.Input;

/// <summary>
/// Platform-specific input device access
/// </summary>
public class InputDeviceAdapter
{
    public bool IsKeyPressed(KeyCode key)
    {
        // Platform-specific key checking
        return false;
    }

    public float GetMouseX() => 0.0f;
    public float GetMouseY() => 0.0f;
}

