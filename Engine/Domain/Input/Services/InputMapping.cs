using Engine.Domain.Input;
using Engine.Domain.Input.ValueObjects;

namespace Engine.Domain.Input.Services;

/// <summary>
/// Domain service for input mapping logic
/// </summary>
public class InputMapping
{
    public InputState MapKeyToState(KeyCode key, bool isPressed)
    {
        return isPressed ? InputState.Pressed : InputState.Released;
    }

    public InputAction? GetActionForBinding(InputBinding binding, KeyCode key)
    {
        if (binding.Key == key)
        {
            return new InputAction { Id = binding.ActionId };
        }
        return null;
    }
}

